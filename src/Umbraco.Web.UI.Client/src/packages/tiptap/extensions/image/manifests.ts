import UmbTiptapImageExtensionApi from './image.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		api: UmbTiptapImageExtensionApi,
		meta: {
			icon: 'icon-picture',
			label: 'Image',
			group: '#tiptap_extGroup_media',
		},
	},
];
