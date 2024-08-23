import type { ManifestFileUploadPreview, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const previews: Array<ManifestFileUploadPreview> = [
	{
		type: 'fileUploadPreview',
		alias: 'Umb.FileUploadPreview.Audio',
		name: 'Audio File Upload Preview',
		element: () => import('./input-upload-field-audio.element.js'),
		forMimeTypes: ['audio/*'],
	},
	{
		type: 'fileUploadPreview',
		alias: 'Umb.FileUploadPreview.File',
		name: 'File File Upload Preview',
		element: () => import('./input-upload-field-file.element.js'),
	},
	{
		type: 'fileUploadPreview',
		alias: 'Umb.FileUploadPreview.Image',
		name: 'Image File Upload Preview',
		element: () => import('./input-upload-field-image.element.js'),
		forMimeTypes: ['image/*'],
	},
	{
		type: 'fileUploadPreview',
		alias: 'Umb.FileUploadPreview.Svg',
		name: 'Svg File Upload Preview',
		element: () => import('./input-upload-field-svg.element.js'),
		forMimeTypes: ['image/svg+xml'],
	},
	{
		type: 'fileUploadPreview',
		alias: 'Umb.FileUploadPreview.Video',
		name: 'Video File Upload Preview',
		element: () => import('./input-upload-field-video.element.js'),
		forMimeTypes: ['video/*'],
	},
];

export const manifests: Array<ManifestTypes> = [...previews];
