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
<xsl:for-each select="$currentPage/* [@isDoc and string(umbracoNaviHide) != '1']">
<div class="photo">

<!-- get first photo thumbnail -->
		<a href="{umbraco.library:NiceUrl(@id)}">
<xsl:if test="count(./* [@isDoc]) &gt; 0">
<img src="{concat(substring-before(./*/umbracoFile,'.'), '_thumb.jpg')}" style="border: none;"/><br/>
</xsl:if>
			<b><xsl:value-of select="@nodeName"/></b><br/>
		</a>
			<xsl:value-of select="count(./* [@isDoc])"/> Photo(s)
</div>
</xsl:for-each>

</xsl:template>

</xsl:stylesheet>