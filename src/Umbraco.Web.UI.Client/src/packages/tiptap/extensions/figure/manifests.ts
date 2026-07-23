export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Figure',
		name: 'Figure Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapFigureExtensionApi })),
		meta: {
			icon: 'icon-frame',
			label: 'Figure',
			group: '#tiptap_extGroup_media',
		},
	},
];
