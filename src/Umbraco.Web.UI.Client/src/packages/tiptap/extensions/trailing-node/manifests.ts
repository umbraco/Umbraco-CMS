export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TrailingNode',
		name: 'Trailing Node Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapTrailingNodeExtensionApi })),
		meta: {
			icon: 'icon-page-down',
			label: 'Trailing Node',
			group: '#tiptap_extGroup_interactive',
		},
	},
];
