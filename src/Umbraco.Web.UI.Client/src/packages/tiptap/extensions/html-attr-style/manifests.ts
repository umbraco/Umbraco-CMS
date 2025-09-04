export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeStyle',
		name: 'Style HTML Attribute Tiptap Extension',
		api: () => import('./html-attr-style.tiptap-api.js'),
		meta: {
			icon: 'icon-palette',
			label: '`style` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
