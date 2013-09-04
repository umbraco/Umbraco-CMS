
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
    <xsl:param name="sortBy" />
    
    <xsl:template match="/">

        <html xmlns="http://www.w3.org/1999/xhtml">
            <head>
                <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
                <title>
                    Export of data from umbraco forms
                </title>
            </head>
        <body>
            <table>
                <caption>Export of data from umbraco forms</caption>
                <thead>
                    <tr>
                        <th scope="col">
                            State
                        </th>
                        <th scope="col">
                            Submitted
                        </th>
                        <th scope="col">
                            Page ID
                        </th>
                        <th scope="col">
                            IP
                        </th>
                        <th scope="col">
                            Member Key
                        </th>
                        <xsl:for-each select="$records//uformrecord[1]/fields/child::*">
                            <xsl:sort select="caption" order="ascending"/>
                            <th scope="col">
                                <xsl:value-of select="normalize-space(caption)"/>
                            </th>
                        </xsl:for-each>
                    </tr>
                </thead>
                <tbody>
                    <xsl:for-each select="$records//uformrecord">
                        <tr>
                            <td>
                                <xsl:value-of select="state"/>
                            </td>
                            
                            <td>
                                <xsl:value-of select="updated"/>
                            </td>
                            
                            <td>
                            <xsl:value-of select="pageid"/>
                            </td>
                            
                            <td>
                            <xsl:value-of select="ip"/>
                            </td>
                            
                            <td>
                            <xsl:value-of select="memberkey"/>
                            </td>
                            
                            <xsl:for-each select="./fields/child::*">
                                <xsl:sort select="caption" order="ascending"/>
                                <td>
                                  <xsl:for-each select="values//value">
                                    <xsl:value-of select="normalize-space(.)"/>
                                    <xsl:if test="position() != last()">,</xsl:if>
                                  </xsl:for-each>
                                </td>
                            </xsl:for-each>
                        </tr>
                    </xsl:for-each>
                </tbody>
            </table>
        </body>
        </html>
    </xsl:template>



</xsl:stylesheet>
