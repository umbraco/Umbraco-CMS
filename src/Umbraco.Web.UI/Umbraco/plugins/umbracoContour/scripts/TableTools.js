/*
 * File:        TableTools.js
 * Version:     1.1.4
 * CVS:         $Id$
 * Description: Copy, save and print functions for DataTables
 * Author:      Allan Jardine (www.sprymedia.co.uk)
 * Created:     Wed  1 Apr 2009 08:41:58 BST
 * Modified:    $Date$ by $Author$
 * Language:    Javascript
 * License:     LGPL
 * Project:     Just a little bit of fun :-)
 * Contact:     www.sprymedia.co.uk/contact
 * 
 * Copyright 2009-2010 Allan Jardine, all rights reserved.
 *
 */

/*
 * Variable: TableToolsInit
 * Purpose:  Parameters for TableTools customisation
 * Scope:    global
 */
var TableToolsInit = {
	"oFeatures": {
		"bCsv": true,
		"bXls": true,
		"bCopy": true,
		"bPrint": true
	},
	"oBom": {
		"bCsv": true,
		"bXls": true
	},
	"bIncFooter": true,
	"bIncHiddenColumns": false,
	"sPrintMessage": "", /* Message with will print with the table */
	"sPrintInfo": "<h6>Print view</h6><p>Please use your browser's print function to "+
		"print this table. Press escape when finished.", /* The 'fading' message */
	"sTitle": "",
	"sSwfPath": "scripts/ZeroClipboard.swf",
	"iButtonHeight": 30,
	"iButtonWidth": 30,
	"sCsvBoundary": "'",
	"_iNextId": 1 /* Internal useage - but needs to be global */
};


(function($) {
/*
 * Function: TableTools
 * Purpose:  TableTools "class"
 * Returns:  same as _fnInit
 * Inputs:   same as _fnInit
 */
function TableTools ( oInit )
{
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private parameters
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	var _oSettings;
	var nTools = null;
	var _nTableWrapper;
	var _aoPrintHidden = [];
	var _iPrintScroll = 0;
	var _nPrintMessage = null;
	var _DTSettings;
	var _sLastData;
	var _iId;
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Initialisation
	 */
	
	/*
	 * Function: _fnInit
	 * Purpose:  Initialise the table tools
	 * Returns:  node: - The created node for the table tools wrapping
 	 * Inputs:   object:oInit - object with:
 	 *             oDTSettings - DataTables settings
	 */
	function _fnInit( oInit )
	{
		_nTools = document.createElement('div');
		_nTools.className = "TableTools";
		_iId = TableToolsInit._iNextId++;
		
		/* Copy the init object */
		_oSettings = $.extend( true, {}, TableToolsInit );
		
		_DTSettings = oInit.oDTSettings;
		
		_nTableWrapper = fnFindParentClass( _DTSettings.nTable, "dataTables_wrapper" );
		
		ZeroClipboard.moviePath = _oSettings.sSwfPath;
		
		if ( _oSettings.oFeatures.bCopy ) {
			fnFeatureClipboard();
		}
		if ( _oSettings.oFeatures.bCsv ) {
			fnFeatureSaveCSV();
		}
		if ( _oSettings.oFeatures.bXls ) {
			fnFeatureSaveXLS();
		}
		if ( _oSettings.oFeatures.bPrint ) {
			fnFeaturePrint();
		}
		
		return _nTools;
	}
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Feature buttons
	 */
	
	/*
	 * Function: fnFeatureSaveCSV
	 * Purpose:  Add a button for saving a CSV file
	 * Returns:  -
	 * Inputs:   -
	 */
	function fnFeatureSaveCSV ()
	{
		var sBaseClass = "TableTools_button TableTools_csv";
		var nButton = document.createElement( 'div' );
		nButton.id = "ToolTables_CSV_"+_iId;
		nButton.style.height = _oSettings.iButtonHeight+'px';
		nButton.style.width = _oSettings.iButtonWidth+'px';
		nButton.className = sBaseClass;
		_nTools.appendChild( nButton );
		
		var clip = new ZeroClipboard.Client();
		clip.setHandCursor( true );
		clip.setAction( 'save' );
		clip.setCharSet( 'UTF8' );
		clip.setBomInc( _oSettings.oBom.bCsv );
		clip.setFileName( fnGetTitle()+'.csv' );
		
		clip.addEventListener('mouseOver', function(client) {
			nButton.className = sBaseClass+'_hover';
		} );
		
		clip.addEventListener('mouseOut', function(client) {
			nButton.className = sBaseClass;
		} );
		
		clip.addEventListener('mouseDown', function(client) {
			fnFlashSetText( clip, fnGetDataTablesData(",", TableToolsInit.sCsvBoundary) );
		} );
		
		fnGlue( clip, nButton, "ToolTables_CSV_"+_iId, "Save as CSV" );
	}
	
	
	/*
	 * Function: fnFeatureSaveXLS
	 * Purpose:  Add a button for saving an XLS file
	 * Returns:  -
	 * Inputs:   -
	 */
	function fnFeatureSaveXLS ()
	{
		var sBaseClass = "TableTools_button TableTools_xls";
		var nButton = document.createElement( 'div' );
		nButton.id = "ToolTables_XLS_"+_iId;
		nButton.style.height = _oSettings.iButtonHeight+'px';
		nButton.style.width = _oSettings.iButtonWidth+'px';
		nButton.className = sBaseClass;
		_nTools.appendChild( nButton );
		
		var clip = new ZeroClipboard.Client();
		clip.setHandCursor( true );
		clip.setAction( 'save' );
		clip.setCharSet( 'UTF16LE' );
		clip.setBomInc( _oSettings.oBom.bXls );
		clip.setFileName( fnGetTitle()+'.xls' );
		
		clip.addEventListener('mouseOver', function(client) {
			nButton.className = sBaseClass+'_hover';
		} );
		
		clip.addEventListener('mouseOut', function(client) {
			nButton.className = sBaseClass;
		} );
		
		clip.addEventListener('mouseDown', function(client) {
			fnFlashSetText( clip, fnGetDataTablesData("\t") );
		} );
		
		fnGlue( clip, nButton, "ToolTables_XLS_"+_iId, "Save for Excel" );
	}
	
	
	/*
	 * Function: fnFeatureClipboard
	 * Purpose:  Add a button for copying data to clipboard
	 * Returns:  -
	 * Inputs:   -
	 */
	function fnFeatureClipboard ()
	{
		var sBaseClass = "TableTools_button TableTools_clipboard";
		var nButton = document.createElement( 'div' );
		nButton.id = "ToolTables_Copy_"+_iId;
		nButton.style.height = _oSettings.iButtonHeight+'px';
		nButton.style.width = _oSettings.iButtonWidth+'px';
		nButton.className = sBaseClass;
		_nTools.appendChild( nButton );
		
		var clip = new ZeroClipboard.Client();
		clip.setHandCursor( true );
		clip.setAction( 'copy' );
		
		clip.addEventListener('mouseOver', function(client) {
			nButton.className = sBaseClass+'_hover';
		} );
		
		clip.addEventListener('mouseOut', function(client) {
			nButton.className = sBaseClass;
		} );
		
		clip.addEventListener('mouseDown', function(client) {
			fnFlashSetText( clip, fnGetDataTablesData("\t") );
		} );
		
		clip.addEventListener('complete', function (client, text) {
			var aData = _sLastData.split('\n');
			alert( 'Copied '+(aData.length-1)+' rows to the clipboard' );
		} );
		
		fnGlue( clip, nButton, "ToolTables_Copy_"+_iId, "Copy to clipboard" );
	}
	
	
	/*
	 * Function: fnFeaturePrint
	 * Purpose:  Add a button for printing data
	 * Returns:  -
	 * Inputs:   -
	 * Notes:    Fun one this function. In order to print the table, we want the table to retain
	 *   it's position in the DOM, so all styles still apply, but we don't want to print all the
	 *   other nonesense. So we hide that nonesese and add an event handler for 'esc' which will
	 *   restore a normal view.
	 */
	function fnFeaturePrint ()
	{
		var sBaseClass = "TableTools_button TableTools_print";
		var nButton = document.createElement( 'div' );
		nButton.style.height = _oSettings.iButtonHeight+'px';
		nButton.style.width = _oSettings.iButtonWidth+'px';
		nButton.className = sBaseClass;
		nButton.title = "Print table";
		_nTools.appendChild( nButton );
		
		/* Could do this in CSS - but might as well be consistent with the flash buttons */
		$(nButton).hover( function(client) {
			nButton.className = sBaseClass+'_hover';
		}, function(client) {
			nButton.className = sBaseClass;
		} );
		
		$(nButton).click( function() {
			/* Parse through the DOM hiding everything that isn't needed for the table */
			fnPrintHideNodes( _DTSettings.nTable );
			
			/* Show the whole table */
			_iPrintSaveStart = _DTSettings._iDisplayStart;
			_iPrintSaveLength = _DTSettings._iDisplayLength;
			_DTSettings._iDisplayStart = 0;
			_DTSettings._iDisplayLength = -1;
			_DTSettings.oApi._fnCalculateEnd( _DTSettings );
			_DTSettings.oApi._fnDraw( _DTSettings );
			
			/* Remove the other DataTables feature nodes - but leave the table! and info div */
			var anFeature = _DTSettings.anFeatures;
			for ( var cFeature in anFeature )
			{
				if ( cFeature != 'i' && cFeature != 't' )
				{
					_aoPrintHidden.push( {
						"node": anFeature[cFeature],
						"display": "block"
					} );
					anFeature[cFeature].style.display = "none";
				}
			}
			
			/* Add a node telling the user what is going on */
			var nInfo = document.createElement( "div" );
			nInfo.className = "TableTools_PrintInfo";
			nInfo.innerHTML = _oSettings.sPrintInfo;
			document.body.appendChild( nInfo );
			
			/* Add a message at the top of the page */
			if ( _oSettings.sPrintMessage !== "" )
			{
				_nPrintMessage = document.createElement( "p" );
				_nPrintMessage.className = "TableTools_PrintMessage";
				_nPrintMessage.innerHTML = _oSettings.sPrintMessage;
				document.body.insertBefore( _nPrintMessage, document.body.childNodes[0] );
			}
			
			/* Cache the scrolling and the jump to the top of the t=page */
			_iPrintScroll = $(window).scrollTop();
			window.scrollTo( 0, 0 );
			
			$(document).bind( "keydown", null, fnPrintEnd );
			
			setTimeout( function() {
				$(nInfo).fadeOut( "normal", function() {
					document.body.removeChild( nInfo );
				} );
			}, 2000 );
		} );
	}
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Printing functions
	 */
	
	/*
	 * Function: fnPrintEnd
	 * Purpose:  Printing is finished, resume normal display
	 * Returns:  -
	 * Inputs:   event
	 */
	function fnPrintEnd ( e )
	{
		/* Only interested in the escape key */
		if ( e.keyCode == 27 )
		{
			/* Show all hidden nodes */
			fnPrintShowNodes();
			
			/* Restore the scroll */
			window.scrollTo( 0, _iPrintScroll );
			
			/* Drop the print message */
			if ( _nPrintMessage )
			{
				document.body.removeChild( _nPrintMessage );
				_nPrintMessage = null;
			}
			
			/* Restore the table length */
			_DTSettings._iDisplayStart = _iPrintSaveStart;
			_DTSettings._iDisplayLength = _iPrintSaveLength;
			_DTSettings.oApi._fnCalculateEnd( _DTSettings );
			_DTSettings.oApi._fnDraw( _DTSettings );
			
			$(document).unbind( "keydown", fnPrintEnd );
		}
	}
	
	
	/*
	 * Function: fnPrintShowNodes
	 * Purpose:  Resume the display of all TableTools hidden nodes
	 * Returns:  -
	 * Inputs:   -
	 */
	function fnPrintShowNodes( )
	{
		for ( var i=0, iLen=_aoPrintHidden.length ; i<iLen ; i++ )
		{
			_aoPrintHidden[i].node.style.display = _aoPrintHidden[i].display;
		}
		_aoPrintHidden.splice( 0, _aoPrintHidden.length );
	}
	
	
	/*
	 * Function: fnPrintHideNodes
	 * Purpose:  Hide nodes which are not needed in order to display the table
	 * Returns:  -
	 * Inputs:   node:nNode - the table node - we parse back up
	 * Notes:    Recursive
	 */
	function fnPrintHideNodes( nNode )
	{
		var nParent = nNode.parentNode;
		var nChildren = nParent.childNodes;
		for ( var i=0, iLen=nChildren.length ; i<iLen ; i++ )
		{
			if ( nChildren[i] != nNode && nChildren[i].nodeType == 1 )
			{
				/* If our node is shown (don't want to show nodes which were previously hidden) */
				var sDisplay = $(nChildren[i]).css("display");
			 	if ( sDisplay != "none" )
				{
					/* Cache the node and it's previous state so we can restore it */
					_aoPrintHidden.push( {
						"node": nChildren[i],
						"display": sDisplay
					} );
					nChildren[i].style.display = "none";
				}
			}
		}
		
		if ( nParent.nodeName != "BODY" )
		{
			fnPrintHideNodes( nParent );
		}
	}
	
	
	
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Support functions
	 */
	
	/*
	 * Function: fnGlue
	 * Purpose:  Wait until the id is in the DOM before we "glue" the swf
	 * Returns:  -
	 * Inputs:   object:clip - Zero clipboard object
	 *           node:node - node to glue swf to
	 *           string:id - id of the element to look for
	 *           string:text - title of the flash movie
	 * Notes:    Recursive (setTimeout)
	 */
	function fnGlue ( clip, node, id, text )
	{
		if ( document.getElementById(id) )
		{
			clip.glue( node, text );
		}
		else
		{
			setTimeout( function () {
				fnGlue( clip, node, id, text );
			}, 100 );
		}
	}
	
	
	/*
	 * Function: fnGetTitle
	 * Purpose:  Get the title of the page (from DOM or user set) for file saving
	 * Returns:  
	 * Inputs:   
	 */
	function fnGetTitle( )
	{
		var sTitle;
		if ( _oSettings.sTitle !== "" ) {
			sTitle = _oSettings.sTitle;
		} else {
			sTitle = document.getElementsByTagName('title')[0].innerHTML;
		}
		
		/* Strip characters which the OS will object to - checking for UTF8 support in the scripting
		 * engine
		 */
		if ( "\u00A1".toString().length < 4 ) {
			return sTitle.replace(/[^a-zA-Z0-9_\u00A1-\uFFFF\.,\-_ !\(\)]/g, "");
		} else {
			return sTitle.replace(/[^a-zA-Z0-9_\.,\-_ !\(\)]/g, "");
		}
	}
	
	
	/*
	 * Function: fnFindParentClass
	 * Purpose:  Parse back up the DOM to a node with a particular node
	 * Returns:  node: - found node
	 * Inputs:   node:n - Node to test
	 *           string:sClass - class to find
	 * Notes:    Recursive
	 */
	function fnFindParentClass ( n, sClass )
	{
		if ( n.className.match(sClass) || n.nodeName == "BODY" )
		{
			return n;
		}
		else
		{
			return fnFindParentClass( n.parentNode, sClass );
		}
	}
	
	
	/*
	 * Function: fnBoundData
	 * Purpose:  Wrap data up with a boundary string
	 * Returns:  string: - bound data
	 * Inputs:   string:sData - data to bound
	 *           string:sBoundary - bounding char(s)
	 *           regexp:regex - search for the bounding chars - constructed outside for efficincy
	 *             in the loop
	 */
	function fnBoundData( sData, sBoundary, regex )
	{
		if ( sBoundary === "" )
		{
			return sData;
		}
		else
		{
			return sBoundary + sData.replace(regex, "\\"+sBoundary) + sBoundary;
		}
	}
	
	
	/*
	 * Function: fnHtmlDecode
	 * Purpose:  Decode HTML entities
	 * Returns:  string: - decoded string
	 * Inputs:   string:sData - encoded string
	 */
	function fnHtmlDecode( sData )
	{
		var 
			aData = fnChunkData( sData, 2048 ),
			n = document.createElement('div'),
			i, iLen, iIndex,
			sReturn = "", sInner;
		
		/* nodeValue has a limit in browsers - so we chunk the data into smaller segments to build
		 * up the string. Note that the 'trick' here is to remember than we might have split over
		 * an HTML entity, so we backtrack a little to make sure this doesn't happen
		 */
		for ( i=0, iLen=aData.length ; i<iLen ; i++ )
		{
			/* Magic number 8 is because no entity is longer then strlen 8 in ISO 8859-1 */
			iIndex = aData[i].lastIndexOf( '&' );
			if ( iIndex != -1 && aData[i].length >= 8 && iIndex > aData[i].length - 8 )
			{
				sInner = aData[i].substr( iIndex );
				aData[i] = aData[i].substr( 0, iIndex );
			}
			
			n.innerHTML = aData[i];
			sReturn += n.childNodes[0].nodeValue;
		}
		
		return sReturn;
	}
	
	
	//function fnHtmlDecode( sData )
	//{
	//	var n = document.createElement('div');
	//	n.innerHTML = sData;
	//	return n.childNodes[0].nodeValue;
	//}
	
	
	/*
	 * Function: fnChunkData
	 * Purpose:  Break a string up into an array of smaller strings
	 * Returns:  array strings: - string array
	 * Inputs:   string:sData - data to be broken up
	 *           int:iSize - chunk size
	 */
	function fnChunkData( sData, iSize )
	{
		var asReturn = [];
		var iStrlen = sData.length;
		
		for ( var i=0 ; i<iStrlen ; i+=iSize )
		{
			if ( i+iSize < iStrlen )
			{
				asReturn.push( sData.substring( i, i+iSize ) );
			}
			else
			{
				asReturn.push( sData.substring( i, iStrlen ) );
			}
		}
		
		return asReturn;
	}
	
	
	/*
	 * Function: fnFlashSetText
	 * Purpose:  Set the text for the flash clip to deal with
	 * Returns:  -
	 * Inputs:   object:clip - the ZeroClipboard object
	 *           string:sData - the data to be set
	 * Notes:    This function is required for large information sets. There is a limit on the 
	 *   amount of data that can be transfered between Javascript and Flash in a single call, so
	 *   we use this method to build up the text in Flash by sending over chunks. It is estimated
	 *   that the data limit is around 64k, although it is undocuments, and appears to be different
	 *   between different flash versions. We chunk at 8KiB.
	 */
	function fnFlashSetText( clip, sData )
	{
		var asData = fnChunkData( sData, 8192 );
		
		clip.clearText();
		for ( var i=0, iLen=asData.length ; i<iLen ; i++ )
		{
			clip.appendText( asData[i] );
		}
	}
	
	
	/*
	 * Function: fnGetDataTablesData
	 * Purpose:  Get data from DataTables' internals and format it for output
	 * Returns:  string:sData - concatinated string of data
	 * Inputs:   string:sSeperator - field separator (ie. ,)
	 *           string:sBoundary - field boundary (ie. ') - optional - default: ""
	 */
	function fnGetDataTablesData( sSeperator, sBoundary )
	{
		var i, iLen;
		var j, jLen;
		var sData = '';
		var sLoopData = '';
		var sNewline = navigator.userAgent.match(/Windows/) ? "\r\n" : "\n";
		
		if ( typeof sBoundary == "undefined" )
		{
			sBoundary = "";
		}
		var regex = new RegExp(sBoundary, "g"); /* Do it here for speed */
		
		/* Titles */
		for ( i=0, iLen=_DTSettings.aoColumns.length ; i<iLen ; i++ )
		{
			if ( _oSettings.bIncHiddenColumns === true || _DTSettings.aoColumns[i].bVisible )
			{
				sLoopData = _DTSettings.aoColumns[i].sTitle.replace(/\n/g," ").replace( /<.*?>/g, "" );
				if ( sLoopData.indexOf( '&' ) != -1 )
				{
					sLoopData = fnHtmlDecode( sLoopData );
				}
				
				sData += fnBoundData( sLoopData, sBoundary, regex ) + sSeperator;
			}
		}
		sData = sData.slice( 0, sSeperator.length*-1 );
		sData += sNewline;
		
		/* Rows */
		for ( j=0, jLen=_DTSettings.aiDisplay.length ; j<jLen ; j++ )
		{
			/* Columns */
			for ( i=0, iLen=_DTSettings.aoColumns.length ; i<iLen ; i++ )
			{
				if ( _oSettings.bIncHiddenColumns === true || _DTSettings.aoColumns[i].bVisible )
				{
					/* Convert to strings (with small optimisation) */
					var mTypeData = _DTSettings.aoData[ _DTSettings.aiDisplay[j] ]._aData[ i ];
					if ( typeof mTypeData == "string" )
					{
						/* Strip newlines, replace img tags with alt attr. and finally strip html... */
						sLoopData = mTypeData.replace(/\n/g," ");
						sLoopData = sLoopData.replace(/<img.*?\s+alt\s*=\s*(?:"([^"]+)"|'([^']+)'|([^\s>]+)).*?>/gi, '$1$2$3')
						sLoopData = sLoopData.replace( /<.*?>/g, "" );
					}
					else
					{
						sLoopData = mTypeData+"";
					}
					
					/* Trim and clean the data */
					sLoopData = sLoopData.replace(/^\s+/, '').replace(/\s+$/, '');
					if ( sLoopData.indexOf( '&' ) != -1 )
					{
						sLoopData = fnHtmlDecode( sLoopData );
					}
					
					/* Bound it and add it to the total data */
					sData += fnBoundData( sLoopData, sBoundary, regex ) + sSeperator;
				}
			}
			sData = sData.slice( 0, sSeperator.length*-1 );
			sData += sNewline;
		}
		
		/* Remove the last new line */
		sData.slice( 0, -1 );
		
		/* Add the footer */
		if ( _oSettings.bIncFooter )
		{
			for ( i=0, iLen=_DTSettings.aoColumns.length ; i<iLen ; i++ )
			{
				if ( _DTSettings.aoColumns[i].nTf !== null &&
					(_oSettings.bIncHiddenColumns === true || _DTSettings.aoColumns[i].bVisible) )
				{
					sLoopData = _DTSettings.aoColumns[i].nTf.innerHTML.replace(/\n/g," ").replace( /<.*?>/g, "" );
					if ( sLoopData.indexOf( '&' ) != -1 )
					{
						sLoopData = fnHtmlDecode( sLoopData );
					}
					
					sData += fnBoundData( sLoopData, sBoundary, regex ) + sSeperator;
				}
			}
			sData = sData.slice( 0, sSeperator.length*-1 );
		}
		
		/* No pointers here - this is a string copy :-) */
		_sLastData = sData;
		return sData;
	}
	
	
	/* Initialise our new object */
	return _fnInit( oInit );
}


/*
 * Register a new feature with DataTables
 */
if ( typeof $.fn.dataTable == "function" && typeof $.fn.dataTableExt.sVersion != "undefined" )
{
	$.fn.dataTableExt.aoFeatures.push( {
		"fnInit": function( oSettings ) {
			return new TableTools( { "oDTSettings": oSettings } );
		},
		"cFeature": "T",
		"sFeature": "TableTools"
	} );
}
else
{
	alert( "Warning: TableTools requires DataTables 1.5 or greater - "+
		"www.datatables.net/download");
}
})(jQuery);
