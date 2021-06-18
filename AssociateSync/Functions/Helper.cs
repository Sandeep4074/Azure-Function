using Microsoft.Azure.WebJobs.Host;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public static class Helper
  {
    public static string GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }

    public static string DbUniqueNumber(string arg, ILogger log)
    {
        string sConn = Helper.GetEnvironmentVariable("TargetConnect");
        string guid = System.Guid.NewGuid().ToString(); ;

        try
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(sConn))
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@pEmplID", SqlDbType.VarChar, 11).Value = arg;
                cmd.CommandText = "p_GetPubID";

                guid = cmd.ExecuteScalar().ToString();

                conn.Close();
            }
        }
        catch (SqlException sex)
        {
            //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "SQL Exception caught.  Detailed exception error:\r\n" + sex.Message, System.Diagnostics.EventLogEntryType.Error);
            log.LogError(sex, "SQL Exception caught.  Detailed exception error:\r\n" + sex.Message);
            System.ArgumentException argEx = new System.ArgumentException("Map script SQL Exception caught.  Detailed exception error: " + sex.Message);
            throw argEx;
        }
        catch (Exception ex)
        {
            //System.Diagnostics.EventLog.WriteEntry("DSG.AssociateSynch", "General Exception caught.  Detailed exception error:\r\n" + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            log.LogError(ex, "General Exception caught.  Detailed exception error:\r\n" + ex.Message);
            System.ArgumentException argEx = new System.ArgumentException("Map script General Exception caught.  Detailed exception error: " + ex.Message);
            throw argEx;
        }
        finally
        {
            //string sDefault = (string)rk.GetValue("AssociateSynch");
            string rk = Helper.GetEnvironmentVariable("WriteEvent");     //SSOSettingsFileReader.ReadString("DSG.AssociateSynch", "WriteEvent");
            if (rk != "No")
            {
                // 2007/02/05 BDR Reduced the footprint of the WriteEvent
                // Write an informational entry to the event log.    
                //EventLog.WriteEntry("BizTalk Server 2013 R2", "AssociateSynch: " + sConn + " : " + guid, EventLogEntryType.Information);
                log.LogInformation("BizTalk Server 2013 R2", "AssociateSynch: " + sConn + " : " + guid);
            }
        }
        return guid;
    }
}