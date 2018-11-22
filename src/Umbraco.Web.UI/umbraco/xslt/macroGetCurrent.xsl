<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
      xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
   <xsl:template match="//nodes/node">
	
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
	</xsl:element>

		
		
   </xsl:template>
</xsl:stylesheet>
