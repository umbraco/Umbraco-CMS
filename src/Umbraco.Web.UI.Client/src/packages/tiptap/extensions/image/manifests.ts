export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapImageExtensionApi })),
		meta: {
			icon: 'icon-picture',
			label: 'Image',
			group: '#tiptap_extGroup_media',
		},
	},
];
