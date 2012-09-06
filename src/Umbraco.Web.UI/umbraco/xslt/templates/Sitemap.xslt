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

<!-- update this variable on how deep your site map should be -->
<xsl:variable name="maxLevelForSitemap" select="4"/>

<xsl:template match="/">
<div id="sitemap"> 
<xsl:call-template name="drawNodes">  
<xsl:with-param name="parent" select="$currentPage/ancestor-or-self::node [@level=1]"/>  
</xsl:call-template>
</div>
</xsl:template>

<xsl:template name="drawNodes">
<xsl:param name="parent"/> 
<xsl:if test="umbraco.library:IsProtected($parent/@id, $parent/@path) = 0 or (umbraco.library:IsProtected($parent/@id, $parent/@path) = 1 and umbraco.library:IsLoggedOn() = 1)">
<ul><xsl:for-each select="$parent/node [string(./data [@alias='umbracoNaviHide']) != '1' and @level &lt;= $maxLevelForSitemap]"> 
<li>  
<a href="{umbraco.library:NiceUrl(@id)}">
<xsl:value-of select="@nodeName"/></a>  
<xsl:if test="count(./node [string(./data [@alias='umbracoNaviHide']) != '1' and @level &lt;= $maxLevelForSitemap]) &gt; 0">   
<xsl:call-template name="drawNodes">    
<xsl:with-param name="parent" select="."/>    
</xsl:call-template>  
</xsl:if> 
</li>
</xsl:for-each>
</ul>
</xsl:if>
</xsl:template>
</xsl:stylesheet>