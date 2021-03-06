<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var ScriptNS0 userCSharp" version="1.0" xmlns:ns3="http://schemas.microsoft.com/Sql/2008/05/ProceduresResultSets/dbo/p_StorePSRecord" xmlns:ns0="http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  	<msxsl:script language="C#" implements-prefix="ScriptNS0">
		<msxsl:assembly name="PSHRtoAM.Helper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=338c006f75a3809b" />
		<msxsl:using namespace="PSHRtoAM.Helper" />
		<![CDATA[public string FormatName(string val1, string val2){ NameFormatter helper = new NameFormatter(); return helper.FormatName(val1, val2); }]]>
	</msxsl:script>
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/DSG_POS_MSG" />
  </xsl:template>
  <xsl:template match="/DSG_POS_MSG">
    <xsl:variable name="var:v4" select="string(MsgData/Transaction/DSG_DERIVED_POS/LAST_NAME/text())" />
    <xsl:variable name="var:v6" select="userCSharp:StringSize(string(MsgData/Transaction/DSG_DERIVED_POS/START_DATE/text()))" />
    <xsl:variable name="var:v7" select="userCSharp:LogicalGt(string($var:v6) , &quot;0&quot;)" />
    <xsl:variable name="var:v8" select="userCSharp:StringSize(string(MsgData/Transaction/DSG_DERIVED_POS/EXPECTED_END_DATE/text()))" />
    <xsl:variable name="var:v9" select="userCSharp:LogicalGt(string($var:v8) , &quot;0&quot;)" />
    <ns0:p_StorePSRecord>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/DEPTID">
        <ns0:DEPTID>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/DEPTID/text()" />
        </ns0:DEPTID>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/DESCR">
        <ns0:DEPT_DESCR>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/DESCR/text()" />
        </ns0:DEPT_DESCR>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/EMPLID">
        <ns0:EMPLID>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/EMPLID/text()" />
        </ns0:EMPLID>
      </xsl:if>
      <xsl:variable name="var:v1" select="ScriptNS0:FormatName(string(MsgData/Transaction/DSG_DERIVED_POS/FIRST_NAME/text()) , &quot;FirstName&quot;)" />
      <ns0:FIRST_NAME>
        <xsl:value-of select="$var:v1" />
      </ns0:FIRST_NAME>
      <xsl:variable name="var:v2" select="ScriptNS0:FormatName(string(MsgData/Transaction/DSG_DERIVED_POS/MIDDLE_NAME/text()) , &quot;MiddleName&quot;)" />
      <ns0:MIDDLE_NAME>
        <xsl:value-of select="$var:v2" />
      </ns0:MIDDLE_NAME>
      <xsl:variable name="var:v3" select="ScriptNS0:FormatName(string(MsgData/Transaction/DSG_DERIVED_POS/LAST_NAME/text()) , &quot;LastName&quot;)" />
      <ns0:LAST_NAME>
        <xsl:value-of select="$var:v3" />
      </ns0:LAST_NAME>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/JOBCODE_DESCR">
        <ns0:JOB_DESCR>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/JOBCODE_DESCR/text()" />
        </ns0:JOB_DESCR>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/DSG_BIRTHDATE_MMDD">
        <ns0:BIRTHDATE>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/DSG_BIRTHDATE_MMDD/text()" />
        </ns0:BIRTHDATE>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/EMPL_STATUS">
        <ns0:EMPL_STATUS>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/EMPL_STATUS/text()" />
        </ns0:EMPL_STATUS>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/JOBCODE">
        <ns0:JOBCODE>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/JOBCODE/text()" />
        </ns0:JOBCODE>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/NATIONAL_ID">
        <ns0:NATIONAL_ID>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/NATIONAL_ID/text()" />
        </ns0:NATIONAL_ID>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/POSTAL">
        <ns0:POSTAL>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/POSTAL/text()" />
        </ns0:POSTAL>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/EMPL_TYPE">
        <ns0:EMPL_TYPE>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/EMPL_TYPE/text()" />
        </ns0:EMPL_TYPE>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/ACCT_CD">
        <ns0:ACCT_CD>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/ACCT_CD/text()" />
        </ns0:ACCT_CD>
      </xsl:if>
      <xsl:variable name="var:v5" select="ScriptNS0:FormatName($var:v4 , &quot;Suffix&quot;)" />
      <ns0:Suffix>
        <xsl:value-of select="$var:v5" />
      </ns0:Suffix>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/LOCATION">
        <ns0:Location>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/LOCATION/text()" />
        </ns0:Location>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/BUSINESS_UNIT">
        <ns0:BusinessUnit>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/BUSINESS_UNIT/text()" />
        </ns0:BusinessUnit>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/PER_ORG">
        <ns0:PER_ORG>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/PER_ORG/text()" />
        </ns0:PER_ORG>
      </xsl:if>
      <xsl:call-template name="StartDateTemplate">
        <xsl:with-param name="condition1" select="string($var:v7)" />
        <xsl:with-param name="value2" select="string(MsgData/Transaction/DSG_DERIVED_POS/START_DATE/text())" />
      </xsl:call-template>
      <xsl:call-template name="ExpEndDateTemplate">
        <xsl:with-param name="condition1" select="string($var:v9)" />
        <xsl:with-param name="value2" select="string(MsgData/Transaction/DSG_DERIVED_POS/EXPECTED_END_DATE/text())" />
      </xsl:call-template>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/MANAGER_ID">
        <ns0:MANAGER_ID>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/MANAGER_ID/text()" />
        </ns0:MANAGER_ID>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/LOCATION_DESCR">
        <ns0:LOCATION_DESCR>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/LOCATION_DESCR/text()" />
        </ns0:LOCATION_DESCR>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/DIVISION">
        <ns0:DIVISION>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/DIVISION/text()" />
        </ns0:DIVISION>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/DSG_DIVISION_DESCR">
        <ns0:DSG_DIVISION_DESCR>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/DSG_DIVISION_DESCR/text()" />
        </ns0:DSG_DIVISION_DESCR>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/SAL_ADMIN_PLAN">
        <ns0:SAL_ADMIN_PLAN>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/SAL_ADMIN_PLAN/text()" />
        </ns0:SAL_ADMIN_PLAN>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/GRADE">
        <ns0:GRADE>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/GRADE/text()" />
        </ns0:GRADE>
      </xsl:if>
      <xsl:if test="MsgData/Transaction/DSG_DERIVED_POS/FLSA_STATUS">
        <ns0:FLSA_STATUS>
          <xsl:value-of select="MsgData/Transaction/DSG_DERIVED_POS/FLSA_STATUS/text()" />
        </ns0:FLSA_STATUS>
      </xsl:if>
    </ns0:p_StorePSRecord>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp"><![CDATA[
public bool LogicalGt(string val1, string val2)
{
	bool ret = false;
	double d1 = 0;
	double d2 = 0;
	if (IsNumeric(val1, ref d1) && IsNumeric(val2, ref d2))
	{
		ret = d1 > d2;
	}
	else
	{
		ret = String.Compare(val1, val2, StringComparison.Ordinal) > 0;
	}
	return ret;
}


public int StringSize(string str)
{
	if (str == null)
	{
		return 0;
	}
	return str.Length;
}


public bool IsNumeric(string val)
{
	if (val == null)
	{
		return false;
	}
	double d = 0;
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}

public bool IsNumeric(string val, ref double d)
{
	if (val == null)
	{
		return false;
	}
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}


]]></msxsl:script>
  <xsl:template name="ExpEndDateTemplate">
<xsl:param name="condition1" />
<xsl:param name="value2" />

<xsl:if test="$condition1='true'">
	<xsl:element name="ns0:EXPECTED_END_DATE">
		<xsl:value-of select="$value2" />
	</xsl:element>
</xsl:if>
</xsl:template>
  <xsl:template name="StartDateTemplate">
<xsl:param name="condition1" />
<xsl:param name="value2" />

<xsl:if test="$condition1='true'">
	<xsl:element name="ns0:START_DATE">
		<xsl:value-of select="$value2" />
	</xsl:element>
</xsl:if>
</xsl:template>
</xsl:stylesheet>