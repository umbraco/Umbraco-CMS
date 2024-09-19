import type { ManifestTiptapExtension } from './tiptap-extension.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const kinds: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Button',
		matchKind: 'button',
		matchType: 'tiptapExtension',
		manifest: {
			element: () => import('./tiptap-toolbar-button.element.js'),
		},
	},
];

const extensions: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		weight: 1000,
		api: () => import('./tiptap-image.extension.js'),
		meta: {},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		weight: 900,
		api: () => import('./tiptap-mediapicker.extension.js'),
		meta: {
			alias: 'umb-media',
			icon: 'icon-picture',
			label: 'Media picker',
		},
	},
];

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...kinds, ...extensions];
