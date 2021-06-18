using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DSG
{
    public static class AssociateSynch
    {
        [FunctionName("AssociateSynch")]
        public static async Task RunAsync([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string logicAppUri = Helper.GetEnvironmentVariable("LogicAppUri");
            HttpClient httpClient = new HttpClient();
            
            CheckAssociateDiff syncher = new DSG.CheckAssociateDiff(logger);
            string resultSet = await syncher.RunSynch();

            if (syncher.DiffRecordCount > 0)
            {
                var response = await httpClient.PostAsync(logicAppUri, new StringContent(resultSet, Encoding.UTF8, "application/xml"));
                var respJsonString = await response.Content.ReadAsStringAsync();
                logger.LogInformation(respJsonString);
            }
            else
            {
                logger.LogInformation("There are no records to send.");
            }
        }
    }

    [Serializable]
    public class CheckAssociateDiff
    {
        ILogger _logger = null;
        private struct tKeyRecord
        {
            public string FieldName;
            public string FieldType;
            public long FieldLength;
        }

        private struct tDataRecord
        {
            public string SourceField;
            public string TargetField;
        }

        private struct tDifference
        {
            public string KeyField;
            public string Status;
        }

        private static string mcClassName = "DSG.AssociateDiff";
        private static string mcRootXML = "AssociateDifference";

        private static string gconCEDT_Mismatch = "Mismatch";
        private static string gconCEDT_SrcMissing = "SourceMissing";
        private static string gconCEDT_TgtMissing = "TargetMissing";

        private bool blnSourceEOF;
        private bool blnTargetEOF;
        private string strKeyList;

        private string strSourceDataList;
        private string strSourceKey;
        private string strSourceData;

        private string strTargetDataList;
        private string strTargetKey;
        private string strTargetData;

        private StringBuilder sXmlMessage;

        private long lngCount;
        private long lngSourceCount;
        private long lngTargetCount;

        private string strHighVal;

        private tKeyRecord[] aKeyList;
        private tDataRecord[] aDataList;
        private tDifference[] aDifferences;
        private long lngRecordCount = 0;

        public long DiffRecordCount { get { return lngRecordCount; } }

        public CheckAssociateDiff(ILogger logger)
        {
            aKeyList = new tKeyRecord[0];
            aDataList = new tDataRecord[0];
            aDifferences = new tDifference[0];
            _logger = logger;
        }

        private string Nz(string strExpression)
        {
            return strExpression;
        }

        private string Nz(string strExpression, string strNull)
        {
            if (strExpression.Length == 0)
                return strNull;
            else
                return strExpression;
        }

        // This function is incomplete for a more robusst implementation
        private string Format(string strExpression, string strFormat)
        {
            string result;
            string format;

            if (strFormat[0] == '0')
            {
                format = "{0:D}";
                result = String.Format(format, strExpression).PadLeft(strFormat.Length, '0');
                result.Substring(result.Length - strFormat.Length);
            }
            else
                format = string.Empty;

            result = String.Format(format, strExpression);

            return result;
        }

        // This function is incomplete for a more robusst implementation
        private int DB2StrComp(string strTargetKey, string strSourceKey, bool blnIsDB2)
        {
            if (blnIsDB2)
                throw new ApplicationException("DB2 String Compare not implemented.");
            else
                return String.Compare(strTargetKey, strSourceKey);
        }

        private void ReadSource(Oracle.ManagedDataAccess.Client.OracleDataReader rstSource)
        {
            int intField;

            if (!rstSource.Read())
            {
                blnSourceEOF = true;

                strSourceKey = strHighVal;
                strSourceData = string.Empty;
            }
            else
            {
                ++lngSourceCount;
                ++lngCount;

                strSourceKey = string.Empty;
                for (intField = 0; intField < aKeyList.Length; ++intField)
                {
                    if (aKeyList[intField].FieldType[0] == 'S')
                        strSourceKey = strSourceKey + ","
                          + rstSource[aKeyList[intField].FieldName].ToString().Trim();
                    else
                        strSourceKey = strSourceKey + ","
                          + Format(rstSource[aKeyList[intField].FieldName].ToString(),
                          String.Empty.PadLeft((int)aKeyList[intField].FieldLength, '0'));
                }
                strSourceKey = strSourceKey.Substring(1);

                strSourceData = string.Empty;
                for (intField = 0; intField < aDataList.Length; ++intField)
                    strSourceData = strSourceData + ","
                        + rstSource[aDataList[intField].SourceField].ToString().Trim();

                strSourceData = strSourceData.Substring(1);
            }
        }

        private void ReadTarget(SqlDataReader rstTarget)
        {
            int intField;

            if (!rstTarget.Read())
            {
                blnTargetEOF = true;

                strTargetKey = strHighVal;
                strTargetData = string.Empty;
            }
            else
            {
                ++lngTargetCount;
                ++lngCount;

                strTargetKey = string.Empty;
                for (intField = 0; intField < aKeyList.Length; ++intField)
                {
                    if (aKeyList[intField].FieldType[0] == 'S')
                        strTargetKey = strTargetKey + ","
                            + rstTarget[aKeyList[intField].FieldName].ToString().Trim();
                    else
                        strTargetKey = strTargetKey + ","
                          + Format(rstTarget[aKeyList[intField].FieldName].ToString(),
                          String.Empty.PadLeft((int)aKeyList[intField].FieldLength, '0'));
                }
                strTargetKey = strTargetKey.Substring(1);

                strTargetData = string.Empty;
                for (intField = 0; intField < aDataList.Length; ++intField)
                    strTargetData = strTargetData + ","
                        + rstTarget[aDataList[intField].TargetField].ToString().Trim();

                strTargetData = strTargetData.Substring(1);
            }
            return;
        }


        private string formatConnection(string conn)
        {
            int posPW = conn.IndexOf("Password=");
            if (posPW == -1)
                return conn;
            else
                return conn.Substring(0, posPW);
        }

        public async Task<string> RunSynch()
        {
            string strSourceConnect = string.Empty;
            string strSourceTable = string.Empty;
            string strTargetConnect = string.Empty;
            string strTargetTable = string.Empty;
            string strFieldList = string.Empty;

            Oracle.ManagedDataAccess.Client.OracleConnection conSource;
            Oracle.ManagedDataAccess.Client.OracleCommand cmdSource = new Oracle.ManagedDataAccess.Client.OracleCommand();
            Oracle.ManagedDataAccess.Client.OracleDataReader rstSource;

            SqlConnection conTarget;
            SqlCommand cmdTarget = new SqlCommand();
            SqlDataReader rstTarget;

            string strSQL;
            long lngMissingSource;
            long lngMissingTarget;
            long lngMismatch;
            //long lngRecordCount;
            long lngMaxRecords;
            object vCompare_Timestamp;

            bool blnIsDB2;
            bool debugging;


            vCompare_Timestamp = DateTime.Now;

            try
            {
                //*D* Create some kind of log entry

                debugging = System.Convert.ToBoolean(Helper.GetEnvironmentVariable("Debugging"));     //System.Convert.ToBoolean(SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "Debug"));
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "debugging: " + debugging.ToString(), System.Diagnostics.EventLogEntryType.Warning);

                // PeopleSoft HRMS - Oracle

                string sourcePassword = Helper.GetEnvironmentVariable("SourcePassword");     //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "OraclePwd");

                //var config = ConfigurationManager.AppSettings;

                string sourceServer = Helper.GetEnvironmentVariable("SourceServer");    // Environment.GetEnvironmentVariable("SQLConnection",EnvironmentVariableTarget.Process);    //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "PsHrmsServer");
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "sourceServer: " + sourceServer, System.Diagnostics.EventLogEntryType.Warning);

                strSourceConnect = sourceServer + sourcePassword;
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "strSourceConnect: " + strSourceConnect, System.Diagnostics.EventLogEntryType.Warning);

                strSourceTable = Helper.GetEnvironmentVariable("SourceTable"); //"SYSADM.PS_DSG_MIIS_ALL_VW"; //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "PsHrmsView");
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "PsHrmsView: " + strSourceTable, System.Diagnostics.EventLogEntryType.Warning);

                strTargetConnect = Helper.GetEnvironmentVariable("TargetConnect");     //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "StagingConnection");
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "StagingConnection: " + strTargetConnect, System.Diagnostics.EventLogEntryType.Warning);

                strTargetTable = Helper.GetEnvironmentVariable("TargetTable"); //"dbo.PsEmployee"; //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "StagingTable");
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "StagingTable: " + strTargetTable, System.Diagnostics.EventLogEntryType.Warning);

                strFieldList = Helper.GetEnvironmentVariable("FieldList"); //"ACCT_CD,ACCT_CD,DEPTID,DEPTID,DESCR,DEPT_DESCR,EMPL_STATUS,EMPLSTATUS,EMPL_TYPE,EMPL_TYPE,JOBCODE,JOBCODE,JOBCODE_DESCR,JOB_DESCR,NATIONAL_ID,NATIONALID,POSTAL,POSTAL,LOCATION,LOCATION,DSG_BIRTHDATE_MMDD,BIRTHDATE,BUSINESS_UNIT,BUSINESSUNIT,PER_ORG,PER_ORG,START_DATE,START_DATE,EXPECTED_END_DATE,EXPECTED_END_DATE,MANAGER_ID,MANAGER_ID,LOCATION_DESCR,LOCATION_DESCRIPTION,DIVISION,DIVISION,DSG_DIVISION_DESCR,DIVISION_DESCR,SAL_ADMIN_PLAN,SAL_ADMIN_PLAN,GRADE,GRADE,FLSA_STATUS,FLSA_STATUS"; //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "FieldList");
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "FieldList: " + strFieldList, System.Diagnostics.EventLogEntryType.Warning);

                lngMaxRecords = Convert.ToInt32(Helper.GetEnvironmentVariable("MaxRecords")); //5000;  //int.Parse(SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "MaxRecords"));
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "FieldList: " + strFieldList, System.Diagnostics.EventLogEntryType.Warning);

            }
            catch (Exception ex)
            {
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch.Helper", "General Exception caught while trying to read SSO values.  Detailed exception error:\r\n" + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                _logger.LogError(ex, "General Exception caught while trying to read SSO values.  Detailed exception error:\r\n" + ex.Message);
                System.ArgumentException argEx = new System.ArgumentException("Map script General Exception caught.  Detailed exception error: " + ex.Message);
                throw argEx;
            }

            blnIsDB2 = false;
            if (blnIsDB2)
                strHighVal = "999999";
            else
                strHighVal = "zzzzzz";

            try
            {
                // Set up connections
                //strSourceConnect = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=ora-cs-d-0269 )(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=HR92U)))" + strSourceConnect;
                strSourceConnect = Helper.GetEnvironmentVariable("SourceConnection") + strSourceConnect;
                conSource = new Oracle.ManagedDataAccess.Client.OracleConnection(strSourceConnect);
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "Oracle strSourceConnect: " + strSourceConnect, System.Diagnostics.EventLogEntryType.Warning);
                //_logger.LogInformation("Before Oracle Open - Con string: " + strSourceConnect);
                conSource.Open();
                _logger.LogInformation("After Oracle Open");
                conTarget = new SqlConnection();
                conTarget.ConnectionString = strTargetConnect;
                //_logger.LogInformation("SQL Connection string: " + strTargetConnect);
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "SQL Server strTargetConnect: " + strTargetConnect, System.Diagnostics.EventLogEntryType.Warning);
                //_logger.LogInformation("Before SQL Open");
                conTarget.Open();
                _logger.LogInformation("After SQL Open");
            }
            catch (Exception e)
            {
                // 2007/05/30 BDR Remove password from exception
                string strDescription = "Creating connections: Src: ";

                // 2008/06/16 BDR Factored password strip
                strDescription += formatConnection(strSourceConnect);

                strDescription += " Tgt: ";
                strDescription += formatConnection(strTargetConnect);

                strDescription += "\r\n";
                //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "Exception caught: " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                _logger.LogError(e, "Exception caught: " + e.Message);

                throw new ApplicationException(strDescription + e.ToString(), e);
            }

            lngCount = 0;
            //--------------------------------------------------------
            // Hardcode field lists to replace generic comparison
            Array.Resize(ref aKeyList, 1);

            aKeyList[0].FieldName = "emplid";
            aKeyList[0].FieldType = "String";
            aKeyList[0].FieldLength = 11;

            strKeyList = "emplid";

            Char cDelimiter = ',';
            string[] aFieldList = strFieldList.Split(cDelimiter);

            Array.Resize(ref aDataList, aFieldList.Length / 2);
            int j = 0;

            for (int i = 0; i < aFieldList.Length; i = i + 2, j++)
            {
                aDataList[j].SourceField = aFieldList[i];
                aDataList[j].TargetField = aFieldList[i + 1];
            }

            foreach (tDataRecord tField in aDataList)
            {
                if (tField.SourceField.Length > 0)
                    strSourceDataList += "," + tField.SourceField;
                if (tField.TargetField.Length > 0)
                    strTargetDataList += "," + tField.TargetField;
            }
            strSourceDataList = strSourceDataList.Substring(1);
            strTargetDataList = strTargetDataList.Substring(1);
            //--------------------------------------------------------
            if (strKeyList.Length == 0)
            {
                // 2005/11/21 BDR Added Keyless file compare
                if (strSourceDataList.Length == 0)
                    throw new ApplicationException(mcClassName + ".cmdCompare: Missing Key definition");

                // No Key fields - assuume all Data fields are key
                strKeyList = strSourceDataList;
                strSourceDataList = string.Empty;
            }

            // Set up data readers
            try
            {
                // Source data reader
                strSQL = "SELECT " + strKeyList;
                if (strSourceDataList.Length > 0)
                    strSQL += "," + strSourceDataList;

                strSQL += " FROM " + strSourceTable;
                // if not terminated in PS-HRMS
                // 2009/11/03 J.Guttman - Added criteria L and P 
                // 2010/01/22 J.Guttman - Added criteria D and U
                strSQL += " WHERE Empl_Status IN ('A','T','L','P','D','U')";
                if (debugging)
                    strSQL += " AND ROWNUM < 100";

                strSQL += " ORDER BY " + strKeyList;

                cmdSource.Connection = conSource;
                cmdSource.CommandText = strSQL;
                cmdSource.CommandType = CommandType.Text;

                rstSource = cmdSource.ExecuteReader();

                // Target data reader
                strSQL = "SELECT " + strKeyList;
                if (strTargetDataList.Length > 0)
                    strSQL = strSQL + "," + strTargetDataList;

                strSQL = strSQL + " FROM " + strTargetTable
                  + " ORDER BY " + strKeyList;

                cmdTarget.Connection = conTarget;

                cmdTarget.CommandText = strSQL;
                cmdTarget.CommandType = CommandType.Text;

                rstTarget = cmdTarget.ExecuteReader();

                lngMissingSource = 0;
                lngMissingTarget = 0;
                lngMismatch = 0;
                

                blnSourceEOF = false;
                blnTargetEOF = false;
                ReadSource(rstSource);
                ReadTarget(rstTarget);

                while ((!blnSourceEOF || !blnTargetEOF) && (lngMaxRecords == 0 || lngRecordCount < lngMaxRecords))
                {
                    if (DB2StrComp(strTargetKey, strSourceKey, blnIsDB2) < 0)
                    {
                        // Missing Source - not in PeopleSoft HRMS
                        ++lngMissingSource;

                        /*
                        Array.Resize(ref aDifferences, aDifferences.Length + 1);
                        aDifferences[aDifferences.Length - 1].KeyField = strTargetKey;
                        aDifferences[aDifferences.Length - 1].Status = gconCEDT_SrcMissing;
                        */

                        ReadTarget(rstTarget);
                    }

                    else if (DB2StrComp(strSourceKey, strTargetKey, blnIsDB2) < 0)
                    {
                        // DO not request Terms that are not in staging
                        if (rstSource["Empl_Status"].ToString()[0] != 'T')
                        {
                            // Missing Target - not in PeopleSoft Staging
                            ++lngMissingTarget;
                            ++lngRecordCount;

                            Array.Resize(ref aDifferences, aDifferences.Length + 1);
                            aDifferences[aDifferences.Length - 1].KeyField = strSourceKey;
                            aDifferences[aDifferences.Length - 1].Status = gconCEDT_TgtMissing;
                        }
                        ReadSource(rstSource);
                    }
                    else if ((String.Compare(strTargetKey, strSourceKey) == 0)
                        && (String.Compare(strTargetData, strSourceData) != 0))
                    {
                        // Data mismatch
                        ++lngMismatch;
                        ++lngRecordCount;

                        Array.Resize(ref aDifferences, aDifferences.Length + 1);
                        aDifferences[aDifferences.Length - 1].KeyField = strTargetKey;
                        aDifferences[aDifferences.Length - 1].Status = gconCEDT_Mismatch;

                        ReadSource(rstSource);
                        ReadTarget(rstTarget);
                    }
                    else if ((strTargetKey == strSourceKey) && (strTargetData == strSourceData))
                    {
                        // All is well
                        ReadSource(rstSource);
                        ReadTarget(rstTarget);
                    }
                }
                rstSource.Close();
                rstTarget.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException("Comparing: "
                    + "\n"
                    + e.ToString(), e);
            }
            finally
            {
                // Clean up readers
                //conSource.Close();
                conTarget.Close();
            }

            //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch.Orchestration", System.String.Format("Diff record count = {0} of {1} ", lngRecordCount, lngSourceCount));
            //lngDiffRecordCount = lngRecordCount;
            _logger.LogInformation(System.String.Format("Diff record count = {0} of {1} ", lngRecordCount, lngSourceCount));

            // 2008/08/06 BDR Updated to SourceConnection/targetConnection 
            // 2008/06/16 BDR Added Source 
            sXmlMessage = new StringBuilder("<?xml version=\"1.0\" ?>\n"
              + "<ns0:" + mcRootXML + " xmlns:ns0=\"http://DSG.AssociateSynch.Schemas\">\n"
              + "<StartTime>" + vCompare_Timestamp.ToString() + "</StartTime>\n"
              + "<EndTime>" + DateTime.Now.ToString() + "</EndTime>\n"
              //+ "<SourceConnection>" + formatConnection(strSourceConnect) + "</SourceConnection>\n"
              + "<SourceCount>" + lngSourceCount + "</SourceCount>\n"
              //+ "<TargetConnection>" + formatConnection(strTargetConnect) + "</TargetConnection>\n"
              + "<TargetCount>" + lngTargetCount + "</TargetCount>\n"
              + "<MissingSourceCount>" + lngMissingSource + "</MissingSourceCount>\n"
              + "<MissingTargetCount>" + lngMissingTarget + "</MissingTargetCount>\n"
              + "<MismatchCount>" + lngMismatch + "</MismatchCount>\n"
              + "<Differences>\n");

            for (int intDiff = 0; intDiff < aDifferences.Length; ++intDiff)
                if (string.Compare(aDifferences[intDiff].Status, gconCEDT_SrcMissing) != 0)
                    sXmlMessage.Append(
                      "<Difference Type=\"" + aDifferences[intDiff].Status + "\" EmployeeID=\""
                      + aDifferences[intDiff].KeyField + "\" />\n");

            sXmlMessage.Append("</Differences>\n"
              + "</ns0:" + mcRootXML + ">");

            return sXmlMessage.ToString();
        }
    }

}
