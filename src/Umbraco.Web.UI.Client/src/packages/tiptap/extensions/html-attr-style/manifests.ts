export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeStyle',
		name: 'Style HTML Attribute Tiptap Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapHtmlAttributeStyleExtensionApi })),
		meta: {
			icon: 'icon-palette',
			label: '`style` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
