import UmbTiptapHtmlTagSpanExtensionApi from './html-tag-span.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlTagSpan',
		name: 'Span HTML Tag Tiptap Extension',
		api: UmbTiptapHtmlTagSpanExtensionApi,
		meta: {
			icon: 'icon-document-html',
			label: '`<span>` tags',
			group: '#tiptap_extGroup_html',
		},
	},
];
