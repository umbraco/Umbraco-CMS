function umbracoPasteCheck(tinyMceId) {
    var _content = tinyMCE.getContent();

	if (_content.indexOf('<FONT') > 0 || _content.indexOf('MsoNormal') > 0 || _content.indexOf('mso-') > 0 || _content.indexOf('style=') > 0 ) {

		var template = new Array();

		template['file']   = '../../../../umbraco/plugins/tinymce/paste.aspx';
		template['width']  = 520;
		template['height'] = 400;
    	tinyMCE.openWindow(template, {editor_id : tinyMceId});
	}
}
