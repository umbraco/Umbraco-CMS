export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaUpload',
		name: 'Media Upload Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapMediaUploadExtensionApi })),
		meta: {
			icon: 'icon-image-up',
			label: 'Media Upload',
			group: '#tiptap_extGroup_media',
		},
	},
];
