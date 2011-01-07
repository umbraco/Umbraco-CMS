<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!-- Set up a local connection string -->
  <xsl:template match="/configuration/appSettings/add[@key='umbracoDbDSN']/@value">
    <xsl:attribute name="value">Data Source=.\SQLEXPRESS;Initial Catalog=Dev.Umbraco.4x;integrated security=false;user id=umbraco;pwd=umbraco</xsl:attribute>
  </xsl:template>
  <xsl:template match="/configuration/appSettings/add[@key='umbracoConfigurationStatus']/@value">
    <xsl:attribute name="value"></xsl:attribute>
  </xsl:template>
  
  <!-- Default templates to match anything else -->
  <xsl:template match="@*">
    <xsl:copy/>
  </xsl:template>

  <xsl:template match="node()">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template> 
</xsl:stylesheet>
