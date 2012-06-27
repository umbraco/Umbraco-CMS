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

<!-- Input the documenttype you want here -->
<!-- Typically '1' for topnavigtaion and '2' for 2nd level -->
<!-- Use div elements around this macro combined with css -->
<!-- for styling the navigation -->
<xsl:variable name="level" select="1"/>

<xsl:template match="/">

<!-- The fun starts here -->
<ul>
<xsl:for-each select="$currentPage/ancestor-or-self::* [@isDoc and @level=$level]/* [@isDoc and string(umbracoNaviHide) != '1']">
	<li>
		<a href="{umbraco.library:NiceUrl(@id)}">
			<xsl:if test="$currentPage/ancestor-or-self::*/@id = current()/@id">
				<!-- we're under the item - you can do your own styling here -->
				<xsl:attribute name="class">selected</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="@nodeName"/>
		</a>
	</li>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>