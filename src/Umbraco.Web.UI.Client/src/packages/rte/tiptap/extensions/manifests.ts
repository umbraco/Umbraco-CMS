import type { ManifestTiptapExtension } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./tiptap-mediapicker.extension.js'),
	},
];
