<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:user="urn:my-scripts"
xmlns:umbraco.library="urn:umbraco.library"
exclude-result-prefixes="xsl msxsl user umbraco.library">

  <xsl:output method="html" media-type="text/html" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
  doctype-system="DTD/xhtml1-strict.dtd"
  cdata-section-elements="script style"
  indent="yes"
  encoding="utf-8"/>

  <xsl:param name="records" />

  <xsl:template match="/">

    <h3>Intro</h3>
    <p>
      Hello, this is a sample email using xslt to convert a record into a custom email
    </p>

    <h3>the fields</h3>
    <ul>
      <xsl:for-each select="$records//fields/child::*">
        <li>
          <h4>
            Caption: <xsl:value-of select="./caption"/>
          </h4>
          <p>
            <xsl:value-of select=".//value"/>
          </p>
        </li>
      </xsl:for-each>
    </ul>

    <h3>The actual xml</h3>
    <xsl:copy-of select="$records"/>

  </xsl:template>



</xsl:stylesheet>
