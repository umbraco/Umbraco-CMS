import UmbTiptapHtmlTagDivExtensionApi from './html-tag-div.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlTagDiv',
		name: 'Div HTML Tag Tiptap Extension',
		api: UmbTiptapHtmlTagDivExtensionApi,
		meta: {
			icon: 'icon-document-html',
			label: '`<div>` tags',
			group: '#tiptap_extGroup_html',
		},
	},
];
