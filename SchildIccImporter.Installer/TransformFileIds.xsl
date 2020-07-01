<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  <xsl:output omit-xml-declaration="yes" indent="yes"/>

  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|*" />
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match='wix:File/@Source'>
    <xsl:attribute name='{name()}'>
      <xsl:variable name='sourceDirStart' select='substring-before(., "\")' />
      <xsl:variable name='sourceDirEnd' select='substring-after(., "\")' />
      <xsl:value-of select='concat($sourceDirStart, "..\..\..\Release\netcoreapp3.1\win-x64\publish\" , $sourceDirEnd)'/>
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>