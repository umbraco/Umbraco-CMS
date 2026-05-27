import UmbTiptapHtmlAttributeIdExtensionApi from './html-attr-id.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeId',
		name: 'ID HTML Attribute Tiptap Extension',
		api: UmbTiptapHtmlAttributeIdExtensionApi,
		meta: {
			icon: 'icon-fingerprint',
			label: '`id` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
