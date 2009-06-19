<?xml version="1.0" ?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"

  xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/"
  xmlns:s="http://www.w3.org/2001/XMLSchema"
  xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/"

  xmlns:soap12="http://schemas.xmlsoap.org/wsdl/soap12/"
 >
  <!-- 
  // wsdl.xslt
  // The WSDL to JavaScript transformation.
  // Copyright by Matthias Hertel, http://www.mathertel.de
  // This work is licensed under a Creative Commons Attribution 2.0 Germany License.
  // See http://creativecommons.org/licenses/by/2.0/de/
  // - - - - - 
  // 19.07.2005 optional documentation
  // 20.07.2005 more datatypes and XML Documents 
  // 20.07.2005 more datatypes and XML Documents fixed
  // 03.12.2005 compatible to axis and bea WebServices. Thanks to Thomas Rudin
  // 07.03.2006 Now this xslt is independent of the alias used for the namespace of XMLSchema in the wsdl.
  //            Thanks to AntÃ³nio Cruz for this great trick.
  // 31.03.2006 Bug on xml types fixed.
  // 19.11.2006 supporting (old) RPC encoding.
-->
  <xsl:strip-space elements="*" />

  <xsl:output method="text" version="4.0" />
  <xsl:param name="alias">
    <xsl:value-of select="wsdl:definitions/wsdl:service/@name" />
  </xsl:param>

  <xsl:variable name="XSDPrefix" select="name(//namespace::*[.='http://www.w3.org/2001/XMLSchema'])" />

  <xsl:template match="/">
    // javascript proxy for SOAP based web services
    // by Matthias Hertel
    /* <xsl:value-of select="wsdl:definitions/wsdl:documentation" /> */
    <xsl:for-each select="/wsdl:definitions/wsdl:service/wsdl:port[soap:address]">
      <xsl:call-template name="soapport" />
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="soapport">
    proxies.<xsl:value-of select="$alias" /> = {
    url: "<xsl:value-of select="soap:address/@location" />",
    ns: "<xsl:value-of select="/wsdl:definitions/wsdl:types/s:schema/@targetNamespace" />"
    } // proxies.<xsl:value-of select="$alias" />
    <xsl:text>&#x000D;&#x000A;</xsl:text>

    <xsl:for-each select="/wsdl:definitions/wsdl:binding[@name = substring-after(current()/@binding, ':')]">
      <xsl:call-template name="soapbinding11" />
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="soapbinding11">
    <xsl:variable name="portTypeName" select="substring-after(current()/@type, ':')" />
    <xsl:for-each select="wsdl:operation">
      <xsl:variable name="inputMessageName" select="substring-after(/wsdl:definitions/wsdl:portType[@name = $portTypeName]/wsdl:operation[@name = current()/@name]/wsdl:input/@message, ':')" />
      <xsl:variable name="outputMessageName" select="substring-after(/wsdl:definitions/wsdl:portType[@name = $portTypeName]/wsdl:operation[@name = current()/@name]/wsdl:output/@message, ':')" />
      /* inputMessageName='<xsl:value-of select="$inputMessageName" />', outputMessageName='<xsl:value-of select="$outputMessageName" />'  */

      <xsl:for-each select="/wsdl:definitions/wsdl:portType[@name = $portTypeName]/wsdl:operation[@name = current()/@name]/wsdl:documentation">
        /** <xsl:value-of select="." /> */
      </xsl:for-each>
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" /> = function () { return(proxies.callSoap(arguments)); }
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" />.fname = "<xsl:value-of select="@name" />";
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" />.service = proxies.<xsl:value-of select="$alias" />;
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" />.action = "\"<xsl:value-of select="soap:operation/@soapAction" />\"";
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" />.params = [<xsl:for-each select="/wsdl:definitions/wsdl:message[@name = $inputMessageName]">
        <xsl:call-template name="soapMessage" />
      </xsl:for-each>];
      proxies.<xsl:value-of select="$alias" />.<xsl:value-of select="@name" />.rtype = [<xsl:for-each select="/wsdl:definitions/wsdl:message[@name = $outputMessageName]">
        <xsl:call-template name="soapMessage" />
      </xsl:for-each>];
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="soapElem">
    <xsl:param name="type"/>
    <xsl:param name="name"/>
    <!-- An annotation to comparisation of the types:
      In XPath 1.0 there is no built in function to check if a string matches a specific type declaration.
      The trick with $XSDPrefix and the 2 following variables help out of this.
      Thanks to AntÃ³nio Cruz for this great trick.
      
      This condition works on ASP.NET with ms - extensions available:
        when test="msxsl:namespace-uri($type)='http://www.w3.org/2001/XMLSchema' and msxsl:local-name($type)='boolean'"
      This condition works with XPath 2.0 functions available:, (see http://www.w3.org/TR/xpath-functions/#func-namespace-uri-from-QName)
        when test="namespace-uri-from-QName($type)='http://www.w3.org/2001/XMLSchema' and local-name-from-QName($type)='boolean'"
      -->
    <xsl:variable name="pre" select="substring-before($type, ':')" />
    <xsl:variable name="post" select="substring-after($type, ':')" />
    <xsl:choose>
      <xsl:when test="$pre != '' and $pre!=$XSDPrefix and $pre!='tns'">
        "<xsl:value-of select="$name" />"
      </xsl:when>

      <xsl:when test="$post='string'">
        "<xsl:value-of select="$name" />"
      </xsl:when>

      <xsl:when test="$post='int' or $post='unsignedInt' or $post='short' or $post='unsignedShort'
          or $post='unsignedLong' or $post='long'">
        "<xsl:value-of select="$name" />:int"
      </xsl:when>

      <xsl:when test="$post='double' or $post='float'">
        "<xsl:value-of select="$name" />:float"
      </xsl:when>
      <xsl:when test="$post='dateTime'">
        "<xsl:value-of select="$name" />:date"
      </xsl:when>

      <xsl:when test="$post='boolean'">
        "<xsl:value-of select="$name" />:bool"
      </xsl:when>

      <!-- arrays !-->
      <xsl:when test="$type='tns:ArrayOfString'">
        "<xsl:value-of select="$name" />:s[]"
      </xsl:when>
      <xsl:when test="$type='tns:ArrayOfInt' or $type='tns:ArrayOfUnsignedInt' or $type='tns:ArrayOfShort' or $type='tns:ArrayOfUnsignedShort' or $type='tns:ArrayOfLong' or $type='tns:ArrayOfUnsignedLong'">
        "<xsl:value-of select="$name" />:int[]"
      </xsl:when>
      <xsl:when test="$type='tns:ArrayOfFloat'">
        "<xsl:value-of select="$name" />:float[]"
      </xsl:when>
      <xsl:when test="$type='tns:ArrayOfBoolean'">
        "<xsl:value-of select="$name" />:bool[]"
      </xsl:when>


      <!-- ASP.NET datasets-->
      <xsl:when test="count(./s:complexType/s:sequence/*) > 1">
        "<xsl:value-of select="$name" />:ds"
      </xsl:when>

      <!-- XML Documents -->
      <xsl:when test="./s:complexType/s:sequence/s:any">
        "<xsl:value-of select="$name" />:x"
      </xsl:when>
      <xsl:otherwise>
        "<xsl:value-of select="$name" />"
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="soapMessage">
    <xsl:choose>
      <xsl:when test="wsdl:part[@type]">
        <!-- SOAP RPC encoding -->
        <xsl:for-each select="wsdl:part">
          <xsl:call-template name="soapElem">
            <xsl:with-param name="name" select="@name" />
            <xsl:with-param name="type" select="@type" />
          </xsl:call-template>
          <xsl:if test="position()!=last()">,</xsl:if>
        </xsl:for-each>
      </xsl:when>

      <xsl:otherwise>
        <!-- SOAP Document encoding -->
        <xsl:variable name="inputElementName" select="substring-after(wsdl:part/@element, ':')" />

        <xsl:for-each select="/wsdl:definitions/wsdl:types/s:schema/s:element[@name=$inputElementName]//s:element">
          <xsl:call-template name="soapElem">
            <xsl:with-param name="name" select="@name" />
            <xsl:with-param name="type" select="@type" />
          </xsl:call-template>
          <xsl:if test="position()!=last()">,</xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>