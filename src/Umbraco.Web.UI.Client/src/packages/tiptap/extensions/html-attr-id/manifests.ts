export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeId',
		name: 'ID HTML Attribute Tiptap Extension',
		api: () => import('./html-attr-id.tiptap-api.js'),
		meta: {
			icon: 'icon-fingerprint',
			label: '`id` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
