import type { ManifestTiptapExtension } from './tiptap-extension.js';

export const manifests: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./tiptap-mediapicker.extension.js'),
	},
];
