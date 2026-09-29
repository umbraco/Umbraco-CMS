export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeDataset',
		name: 'Dataset HTML Attribute Tiptap Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapHtmlAttributeDatasetExtensionApi })),
		meta: {
			icon: 'icon-binarycode',
			label: '`data-*` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
