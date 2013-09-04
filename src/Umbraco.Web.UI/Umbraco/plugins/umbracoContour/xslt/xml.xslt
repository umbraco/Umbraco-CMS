<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:user="urn:my-scripts" 
exclude-result-prefixes="xsl msxsl user">

    <xsl:output method="xml" media-type="text/xml"
    cdata-section-elements="script style"
    indent="yes" encoding="utf-8" omit-xml-declaration="yes" version="1.0"/>

    <xsl:param name="records" />

    <xsl:template match="/">
        <xsl:copy-of select="$records"/>
    </xsl:template>

</xsl:stylesheet>
