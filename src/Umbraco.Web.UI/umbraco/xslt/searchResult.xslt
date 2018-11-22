<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <ol>
      <xsl:for-each select="/results/result">
        <li>
          <h4>
            <a href="#" onClick="openItem('{@id}');">
              <!-- add accesskey support for the first 9 results -->
              <xsl:if test="position() &lt; 10">
                <xsl:attribute name="accesskey">
                  <xsl:value-of select="position()"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:value-of select="@title"/>
            </a>
			      <small><strong>Node Type: </strong> <xsl:value-of select="@type"/></small>
          </h4>
          <p>
            <em>Last updated <xsl:value-of select="@author"/> on <xsl:value-of select="@changeDate"/>
          </em>
          </p>
        </li>

      </xsl:for-each>
    </ol>
  </xsl:template>
</xsl:stylesheet>
