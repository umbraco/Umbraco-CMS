import type { ManifestTiptapExtension } from './tiptap-extension.js';

export const manifests: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		weight: 1000,
		api: () => import('./tiptap-image.extension.js'),
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		weight: 900,
		api: () => import('./tiptap-mediapicker.extension.js'),
	},
];
