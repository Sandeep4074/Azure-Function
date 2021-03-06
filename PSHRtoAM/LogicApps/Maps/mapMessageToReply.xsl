<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var userCSharp" version="1.0" xmlns:ns0="http://DSG.PSHRtoAM" xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/DSG_POS_MSG" />
  </xsl:template>
  <xsl:template match="/DSG_POS_MSG">
    <xsl:variable name="var:v1" select="userCSharp:StringConcat(&quot;PublishSubscribe&quot;)" />
    <xsl:variable name="var:v2" select="userCSharp:StringConcat(&quot;PublishSubscribeSystem&quot;)" />
    <xsl:variable name="var:v3" select="userCSharp:StringConcat(&quot;1&quot;)" />
    <xsl:variable name="var:v4" select="userCSharp:StringConcat(&quot;PingNode&quot;)" />
    <xsl:variable name="var:v5" select="userCSharp:StringConcat(&quot;number&quot;)" />
    <xsl:variable name="var:v6" select="userCSharp:StringConcat(&quot;0&quot;)" />
    <ns0:reply>
      <ns0:operations>
        <xsl:attribute name="namespace">
          <xsl:value-of select="$var:v1" />
        </xsl:attribute>
        <xsl:attribute name="interface">
          <xsl:value-of select="$var:v2" />
        </xsl:attribute>
        <ns0:invoke>
          <xsl:attribute name="opnum">
            <xsl:value-of select="$var:v3" />
          </xsl:attribute>
          <xsl:attribute name="member">
            <xsl:value-of select="$var:v4" />
          </xsl:attribute>
          <ns0:return>
            <xsl:attribute name="type">
              <xsl:value-of select="$var:v5" />
            </xsl:attribute>
            <xsl:value-of select="$var:v6" />
          </ns0:return>
        </ns0:invoke>
      </ns0:operations>
    </ns0:reply>
  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp"><![CDATA[
public string StringConcat(string param0)
{
   return param0;
}



]]></msxsl:script>
</xsl:stylesheet>