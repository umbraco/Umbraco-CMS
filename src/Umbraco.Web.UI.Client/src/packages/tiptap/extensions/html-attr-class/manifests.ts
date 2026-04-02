export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeClass',
		name: 'Class HTML Attribute Tiptap Extension',
		api: () => import('./html-attr-class.tiptap-api.js'),
		meta: {
			icon: 'icon-barcode',
			label: '`class` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
