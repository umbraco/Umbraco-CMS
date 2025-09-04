export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Figure',
		name: 'Figure Tiptap Extension',
		api: () => import('./figure.tiptap-api.js'),
		meta: {
			icon: 'icon-frame',
			label: 'Figure',
			group: '#tiptap_extGroup_media',
		},
	},
];
