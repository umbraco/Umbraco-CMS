<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:umbraco.contour="urn:umbraco.contour"
	exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets umbraco.contour ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<!-- To use this sample out of the box, you'll have to have a form on the page that has a 'Name' and a 'Comment' field -->


<!-- Get all approved records of the current page by using the library method GetApprovedRecordsFromPage -->
<xsl:variable name="records" select="umbraco.contour:GetApprovedRecordsFromPage($currentPage/@id)" />

<!-- Display the number of records -->
<div id="recordscount" style="padding-bottom:10px">
	<xsl:choose>
		<xsl:when test="count($records//uformrecord) = 0">
			No comments
		</xsl:when>
		<xsl:when test="count($records//uformrecord) = 1">
			1 comment
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="count($records//uformrecord)"/> comments
		</xsl:otherwise>
	</xsl:choose>
	
</div>

<div id="records">

<!-- Loop all records -->
<xsl:for-each select="$records//uformrecord">
	<div class="record" style="padding-bottom:10px">
		<!-- Display 'Name' field of the record, make sure there is a field called 'Name' on the form -->
		<cite><xsl:value-of select=".//fields/name//value"/> </cite> Says: <br />
		<!-- Display the creation time and format it using the umbraco.library method LongDate, each record has a created node that contains the creation datetimestamp -->
		<small><xsl:value-of select="umbraco.library:LongDate(./created)"/> </small>
		<div>
			<!-- Display 'Comment' field of the record, make sure there is a field called 'Comment' on the form -->
			<xsl:value-of select="umbraco.library:ReplaceLineBreaks(.//fields/comment//value)" disable-output-escaping="yes"/>
		</div>
	</div>
</xsl:for-each>

</div>

</xsl:template>

</xsl:stylesheet>