<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:rssdatehelper="urn:rssdatehelper"
  xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:content="http://purl.org/rss/1.0/modules/content/"
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" {0}
  exclude-result-prefixes="msxml umbraco.library {1}">


  <xsl:output method="xml" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>

  <!-- Update these variables to modify the feed -->
  <xsl:variable name="RSSNoItems" select="string('10')"/>
  <xsl:variable name="RSSTitle" select="string('My sample rss')"/>
  <xsl:variable name="SiteURL" select="string('Add your url here')"/>
  <xsl:variable name="RSSDescription" select="string('Add your description here')"/>

  <!-- This gets all news and events and orders by updateDate to use for the pubDate in RSS feed -->
  <xsl:variable name="pubDate">
    <xsl:for-each select="$currentPage/* [@isDoc]">
      <xsl:sort select="@createDate" data-type="text" order="descending" />
      <xsl:if test="position() = 1">
        <xsl:value-of select="updateDate" />
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <xsl:template match="/">
    <!-- change the mimetype for the current page to xml -->
    <xsl:value-of select="umbraco.library:ChangeContentType('text/xml')"/>

    <xsl:text disable-output-escaping="yes">&lt;?xml version="1.0" encoding="UTF-8"?&gt;</xsl:text>
    <rss version="2.0"
    xmlns:content="http://purl.org/rss/1.0/modules/content/"
    xmlns:wfw="http://wellformedweb.org/CommentAPI/"
    xmlns:dc="http://purl.org/dc/elements/1.1/"
>

      <channel>
        <title>
          <xsl:value-of select="$RSSTitle"/>
        </title>
        <link>
          <xsl:value-of select="$SiteURL"/>
        </link>
        <pubDate>
          <xsl:value-of select="$pubDate"/>
        </pubDate>
        <generator>umbraco</generator>
        <description>
          <xsl:value-of select="$RSSDescription"/>
        </description>
        <language>en</language>

        <xsl:apply-templates select="$currentPage/* [@isDoc and string(umbracoNaviHide) != '1']">
          <xsl:sort select="@createDate" order="descending" />
        </xsl:apply-templates>
      </channel>
    </rss>

  </xsl:template>

  <xsl:template match="* [@isDoc]">
    <xsl:if test="position() &lt;= $RSSNoItems">
      <item>
        <title>
          <xsl:value-of select="@nodeName"/>
        </title>
        <link>
          <xsl:value-of select="$SiteURL"/>
          <xsl:value-of select="umbraco.library:NiceUrl(@id)"/>
        </link>
        <pubDate>
          <xsl:value-of select="umbraco.library:FormatDateTime(@createDate,'r')" />
        </pubDate>
        <guid>
          <xsl:value-of select="$SiteURL"/>
          <xsl:value-of select="umbraco.library:NiceUrl(@id)"/>
        </guid>
        <content:encoded>
          <xsl:value-of select="concat('&lt;![CDATA[ ', ./bodyText,']]&gt;')" disable-output-escaping="yes"/>
        </content:encoded>
      </item>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>