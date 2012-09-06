<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!--

************************************************************
umbraco/xslt/sqlNodeHierachy.xslt
=================================
Copyright 2000-2004 of Niels Hartvig
umbraco is an open source content management platform.
More info: http://www.umbraco.org

************************************************************


DO NOT MODIFY THIS FILE
=======================
This file is used to populate the umbraco xml source.
-->




















<xsl:template match="/">
<xsl:text disable-output-escaping="yes">
&lt;!DOCTYPE umbraco [
  &lt;!ELEMENT nodes ANY&gt; 
  &lt;!ELEMENT node ANY&gt; 
  &lt;!ATTLIST node id ID #REQUIRED&gt;
]&gt;
  </xsl:text>
	<umbraco>
	<nodes>
    <xsl:call-template name="getSubs">
		<xsl:with-param name="parent" select="-1" />
	</xsl:call-template>
	</nodes>
	</umbraco>
</xsl:template>

<xsl:template name="getSubs">
	<xsl:param name="parent"/>
	
	<xsl:for-each select="/root/node [@parentID = $parent]">
	<xsl:element name="{name()}">
		<xsl:for-each select="@*">
			<xsl:attribute name="{name()}"><xsl:value-of select="."/></xsl:attribute>
		</xsl:for-each>
		<xsl:for-each select="./data">
			<xsl:element name="{name()}">
				<xsl:for-each select="@*">
					<xsl:attribute name="{name()}"><xsl:value-of select="."/></xsl:attribute>
				</xsl:for-each>
				<xsl:value-of select="."/>
			</xsl:element>
		</xsl:for-each>
	    <xsl:call-template name="getSubs">
			<xsl:with-param name="parent" select="@id" />
		</xsl:call-template>
	</xsl:element>

		
	</xsl:for-each>
		
</xsl:template>

</xsl:stylesheet>

  