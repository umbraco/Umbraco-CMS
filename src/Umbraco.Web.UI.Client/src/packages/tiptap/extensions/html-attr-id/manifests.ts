export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeId',
		name: 'ID HTML Attribute Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapHtmlAttributeIdExtensionApi })),
		meta: {
			icon: 'icon-fingerprint',
			label: '`id` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
