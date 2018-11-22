<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt" 
	xmlns:umbraco.library="urn:umbraco.library" {0}
	exclude-result-prefixes="msxml umbraco.library {1}">

<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<!-- The fun starts here -->
<xsl:for-each select="$currentPage/node [string(data [@alias='umbracoNaviHide']) != '1']">
<div style="text-align: center; width: 150px; height: 125px; float: left; border: 1px solid #999; background-color: #EDEDED; padding: 5px; margin: 10px;" onMouseOver="this.style.backgroundColor='CCC';" onMouseOut="this.style.backgroundColor='#EDEDED';">

<!-- get first photo thumbnail -->
		<a href="{umbraco.library:NiceUrl(@id)}">
<xsl:if test="count(./node) &gt; 0">
<img src="{concat(substring-before(./node/data [@alias='umbracoFile'],'.'), '_thumb.jpg')}" style="border: none;"/><br/>
</xsl:if>
			<b><xsl:value-of select="@nodeName"/></b><br/>
		</a>
			<xsl:value-of select="count(./node)"/> Photo(s)
</div>
</xsl:for-each>

</xsl:template>

</xsl:stylesheet>