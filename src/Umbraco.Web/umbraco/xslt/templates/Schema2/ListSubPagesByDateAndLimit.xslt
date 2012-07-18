<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt" 
	xmlns:umbraco.library="urn:umbraco.library" {0}
	exclude-result-prefixes="msxml umbraco.library {1}">

<xsl:output method="xml" omit-xml-declaration="yes" />

<xsl:param name="currentPage"/>

<!-- Don't change this but create a 'number' element in your -->
<!-- macro with the alias of 'numberOfItems' -->
<xsl:variable name="numberOfItems" select="/macro/numberOfItems"/>

<xsl:template match="/">

<!-- The fun starts here -->
<ul>
<xsl:for-each select="$currentPage/* [@isDoc and string(umbracoNaviHide) != '1']">
<xsl:sort select="@updateDate" order="descending"/>
	<xsl:if test="position() &lt;= $numberOfItems">
		<li>
			<a href="{umbraco.library:NiceUrl(@id)}">
				<xsl:value-of select="@nodeName"/>
			</a>
		</li>
	</xsl:if>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>