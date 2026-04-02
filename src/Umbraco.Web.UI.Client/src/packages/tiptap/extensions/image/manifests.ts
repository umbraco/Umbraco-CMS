export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		api: () => import('./image.tiptap-api.js'),
		meta: {
			icon: 'icon-picture',
			label: 'Image',
			group: '#tiptap_extGroup_media',
		},
	},
];
