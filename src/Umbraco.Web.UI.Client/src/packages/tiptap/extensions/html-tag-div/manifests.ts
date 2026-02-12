export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlTagDiv',
		name: 'Div HTML Tag Tiptap Extension',
		api: () => import('./html-tag-div.tiptap-api.js'),
		meta: {
			icon: 'icon-document-html',
			label: '`<div>` tags',
			group: '#tiptap_extGroup_html',
		},
	},
];
