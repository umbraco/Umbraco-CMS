import UmbTiptapMediaUploadExtensionApi from './media-upload.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaUpload',
		name: 'Media Upload Tiptap Extension',
		api: UmbTiptapMediaUploadExtensionApi,
		meta: {
			icon: 'icon-image-up',
			label: 'Media Upload',
			group: '#tiptap_extGroup_media',
		},
	},
];
