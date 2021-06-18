<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var s0 userCSharp ScriptNS0 ScriptNS1" version="1.0" xmlns:s0="http://DSG.AssociateSynch.Schemas" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp" xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
	<xsl:param name="FromNode"/>
	<xsl:param name="ToNode"/>
	<xsl:param name="DBUniqueNo"/>
  <xsl:template match="/">
    <xsl:apply-templates select="/s0:AssociateDifference" />
  </xsl:template>
  <xsl:template match="/s0:AssociateDifference">
   <xsl:variable name="var:v10" select="userCSharp:DateCurrentDateTime()" />
    <request>
      <xsl:attribute name="version">
        <xsl:text>1.1</xsl:text>
      </xsl:attribute>
      <from>
        <xsl:attribute name="node">
          <xsl:value-of select="$FromNode" />
        </xsl:attribute>
      </from>
      <to>
        <xsl:attribute name="node">
          <xsl:value-of select="$ToNode" />
        </xsl:attribute>
      </to>
      <operations>
        <xsl:attribute name="namespace">
          <xsl:text>PublishSubscribe</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="interface">
          <xsl:text>PublishSubscribeSystem</xsl:text>
        </xsl:attribute>
        <invoke>
          <xsl:attribute name="member">
            <xsl:text>Publish</xsl:text>
          </xsl:attribute>
          <variable>
            <xsl:attribute name="type">
              <xsl:text>object</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="interface">
              <xsl:text>Publication</xsl:text>
            </xsl:attribute>
            <publication>
              <publishingnode>
                <xsl:value-of select="$FromNode" />
              </publishingnode>
              <channel>
                <xsl:text>DSG_POS_MSG_CHNL</xsl:text>
              </channel>
              <publicationid>
                <xsl:value-of select="$DBUniqueNo" />
              </publicationid>
              <subject>
                <xsl:text>DSG_EMPL_REQUEST</xsl:text>
              </subject>
              <class>
                <xsl:text>PSAPMSG</xsl:text>
              </class>
              <originatingnode>
                <xsl:value-of select="$FromNode" />
              </originatingnode>
              <publisher>
                <xsl:text>BizTalk</xsl:text>
              </publisher>
              <publicationprocess>
                <xsl:text>SYNCH_DATA</xsl:text>
              </publicationprocess>
              <publishtimestamp>
                <xsl:value-of select="$var:v10" />
              </publishtimestamp>
              <status>
                <xsl:text>4</xsl:text>
              </status>
              <defaultdataversion>
                <xsl:text>VERSION_1</xsl:text>
              </defaultdataversion>
              <dataversions>
                <publicationdataversion>
                  <version>
                    <xsl:text>VERSION_1</xsl:text>
                  </version>
                  <data>
                    <DSG_EMPL_REQUEST>
                      <FieldTypes>
                        <DSG_DERIVED_EMP>
                          <xsl:attribute name="class">
                            <xsl:text>R</xsl:text>
                          </xsl:attribute>
                          <EMPLID>
                            <xsl:attribute name="type">
                              <xsl:text>CHAR</xsl:text>
                            </xsl:attribute>
                          </EMPLID>
                        </DSG_DERIVED_EMP>
                        <PSCAMA>
                          <xsl:attribute name="class">
                            <xsl:text>R</xsl:text>
                          </xsl:attribute>
                          <LANGUAGE_CD>
                            <xsl:attribute name="type">
                              <xsl:text>CHAR</xsl:text>
                            </xsl:attribute>
                          </LANGUAGE_CD>
                          <AUDIT_ACTN>
                            <xsl:attribute name="type">
                              <xsl:text>CHAR</xsl:text>
                            </xsl:attribute>
                          </AUDIT_ACTN>
                        </PSCAMA>
                      </FieldTypes>
                      <MsgData>
                        <xsl:for-each select="Differences/Difference">
                          <Transaction>
                            <DSG_DERIVED_EMP>
                              <xsl:attribute name="class">
                                <xsl:text>R</xsl:text>
                              </xsl:attribute>
                              <EMPLID>
                                <xsl:value-of select="@EmployeeID" />
                              </EMPLID>
                            </DSG_DERIVED_EMP>
                            <PSCAMA>
                              <xsl:attribute name="class">
                                <xsl:text>R</xsl:text>
                              </xsl:attribute>
                              <LANGUAGE_CD>
                                <xsl:text>ENG</xsl:text>
                              </LANGUAGE_CD>
                              <xsl:variable name="var:v11" select="userCSharp:MyNull()" />
                              <AUDIT_ACTN>
                                <xsl:value-of select="$var:v11" />
                              </AUDIT_ACTN>
                            </PSCAMA>
                            <xsl:value-of select="./text()" />
                          </Transaction>
                        </xsl:for-each>
                        <xsl:value-of select="Differences/text()" />
                      </MsgData>
                    </DSG_EMPL_REQUEST>
                  </data>
                </publicationdataversion>
              </dataversions>
            </publication>
          </variable>
        </invoke>
      </operations>
    </request>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp"><![CDATA[
public string DateCurrentDateTime()
{
	DateTime dt = DateTime.Now;
	string curdate = dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
	string curtime = dt.ToString("T", System.Globalization.CultureInfo.InvariantCulture);
	string retval = curdate + "T" + curtime;
	return retval;
}


// Return null to Audit Action

public string MyNull()
{
	return String.Empty;
}

public string StringConcat(string param0)
{
   return param0;
}



]]></msxsl:script>
</xsl:stylesheet>