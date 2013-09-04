<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:user="urn:my-scripts">    
    
    <xsl:output method="text" indent="no" encoding="utf-8" />

    <xsl:param name="records" />
    
<xsl:template match="/">
"State","Submitted","PageId","IP","MemberId",<xsl:for-each select="$records//uformrecord[1]/fields/child::*"><xsl:sort select="caption" order="ascending"/><xsl:if test="position() != last()">"<xsl:value-of select="normalize-space(translate(caption,',',''))"/>",</xsl:if><xsl:if test="position()  = last()">"<xsl:value-of select="normalize-space(translate(caption,',',''))"/>"<xsl:text>&#xD;</xsl:text></xsl:if></xsl:for-each>
<xsl:for-each select="$records//uformrecord">"<xsl:value-of select="state"/>","<xsl:value-of select="updated"/>","<xsl:value-of select="pageid"/>","<xsl:value-of select="ip"/>","<xsl:value-of select="memberkey"/>",<xsl:for-each select="./fields/child::*"><xsl:sort select="caption" order="ascending"/><xsl:if test="position() != last()"><xsl:choose><xsl:when test="count(values//value) &gt; 1">"<xsl:for-each select="values//value"><xsl:if test="position() != last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/>;</xsl:if><xsl:if test="position() = last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/></xsl:if></xsl:for-each>",</xsl:when><xsl:otherwise>"<xsl:value-of select="normalize-space(translate(values//value,',',''))"/>",</xsl:otherwise></xsl:choose></xsl:if><xsl:if test="position() = last()"><xsl:choose><xsl:when test="count(values//value) &gt; 1">"<xsl:for-each select="values//value"><xsl:if test="position() != last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/>;</xsl:if><xsl:if test="position() = last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/></xsl:if></xsl:for-each>"</xsl:when><xsl:otherwise>"<xsl:value-of select="normalize-space(translate(values//value,',',''))"/>"</xsl:otherwise></xsl:choose><xsl:text>&#xD;</xsl:text></xsl:if></xsl:for-each>
</xsl:for-each>

    
</xsl:template>
    
   
 
</xsl:stylesheet>