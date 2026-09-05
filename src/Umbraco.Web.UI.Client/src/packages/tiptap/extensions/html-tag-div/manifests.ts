export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlTagDiv',
		name: 'Div HTML Tag Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapHtmlTagDivExtensionApi })),
		meta: {
			icon: 'icon-document-html',
			label: '`<div>` tags',
			group: '#tiptap_extGroup_html',
		},
	},
];
