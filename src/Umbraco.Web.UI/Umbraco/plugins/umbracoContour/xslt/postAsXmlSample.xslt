<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:user="urn:my-scripts"
exclude-result-prefixes="xsl msxsl user">

    <xsl:output method="xml" media-type="text/html" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
    doctype-system="DTD/xhtml1-strict.dtd"
    cdata-section-elements="script style"
    indent="yes"
    encoding="utf-8"/>

    <xsl:param name="records" />
   
    <xsl:template match="/">

        <!--
            This shows how xslt can be used to reformat the record xml, before it is send to a url
            using the "post as xml" workflow.
            
            The sample xml is from the unfuddle tiket api, a brilliant way to manage a project
        -->
        <ticket>
            <description>
                <xsl:value-of select="$records/uformrecord/fields/description"/>
            </description>
            <priority>1</priority>
            <project-id type="integer">1</project-id>
            <reporter-id type="integer">1</reporter-id>
            <status>new</status>
            <summary>
                <xsl:value-of select="$records/uformrecord/fields/summary"/>
            </summary>
        </ticket>
    
    </xsl:template>



</xsl:stylesheet>
