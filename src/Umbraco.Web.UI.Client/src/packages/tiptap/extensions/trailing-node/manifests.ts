import UmbTiptapTrailingNodeExtensionApi from './trailing-node.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TrailingNode',
		name: 'Trailing Node Tiptap Extension',
		api: UmbTiptapTrailingNodeExtensionApi,
		meta: {
			icon: 'icon-page-down',
			label: 'Trailing Node',
			group: '#tiptap_extGroup_interactive',
		},
	},
];
