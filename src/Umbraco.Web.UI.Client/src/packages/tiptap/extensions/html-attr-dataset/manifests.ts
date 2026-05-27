import UmbTiptapHtmlAttributeDatasetExtensionApi from './html-attr-dataset.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HtmlAttributeDataset',
		name: 'Dataset HTML Attribute Tiptap Extension',
		api: UmbTiptapHtmlAttributeDatasetExtensionApi,
		meta: {
			icon: 'icon-binarycode',
			label: '`data-*` attributes',
			group: '#tiptap_extGroup_html',
		},
	},
];
