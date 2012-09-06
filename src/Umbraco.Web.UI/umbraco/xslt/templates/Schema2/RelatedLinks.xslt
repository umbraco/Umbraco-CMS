<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [  <!ENTITY nbsp "&#x00A0;">]>
<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxml="urn:schemas-microsoft-com:xslt" 
	xmlns:umbraco.library="urn:umbraco.library" {0}
	exclude-result-prefixes="msxml umbraco.library {1}">


  <xsl:output method="xml" omit-xml-declaration="yes" />

  <xsl:param name="currentPage"/>
  
  <!-- Input the related links property alias here -->
  <xsl:variable name="propertyAlias" select="string('links')"/>
  
  <xsl:template match="/">

    <!-- The fun starts here -->
    <ul>
      <xsl:for-each select="$currentPage/* [name() = $propertyAlias and not(@isDoc)]/links/link">
        <li>
          <xsl:element name="a">
            <xsl:if test="./@newwindow = '1'">
              <xsl:attribute name="target">_blank</xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="./@type = 'external'">
                <xsl:attribute name="href">
                  <xsl:value-of select="./@link"/>
                </xsl:attribute>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="href">
                  <xsl:value-of select="umbraco.library:NiceUrl(./@link)"/>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:value-of select="./@title"/>
          </xsl:element>
        </li>
      </xsl:for-each>
    </ul>

    <!-- Live Editing support for related links. -->
    <xsl:value-of select="umbraco.library:Item($currentPage/@id,$propertyAlias,'')" />

  </xsl:template>

</xsl:stylesheet>