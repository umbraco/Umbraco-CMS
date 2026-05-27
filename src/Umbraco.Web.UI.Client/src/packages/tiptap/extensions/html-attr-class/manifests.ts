import UmbTiptapHtmlAttributeClassExtensionApi from './html-attr-class.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeClass',
		name: 'Class HTML Attribute Tiptap Extension',
		api: UmbTiptapHtmlAttributeClassExtensionApi,
		meta: {
			icon: 'icon-barcode',
			label: '`class` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
