export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TrailingNode',
		name: 'Trailing Node Tiptap Extension',
		api: () => import('./trailing-node.tiptap-api.js'),
		meta: {
			icon: 'icon-page-down',
			label: 'Trailing Node',
			group: '#tiptap_extGroup_interactive',
		},
	},
];
