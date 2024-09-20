import type { ManifestTiptapExtension, ManifestTiptapExtensionButtonKind } from './tiptap-extension.js';
import { manifests as core } from './core/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const kinds: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Button',
		matchKind: 'button',
		matchType: 'tiptapExtension',
		manifest: {
			element: () => import('../components/toolbar/tiptap-toolbar-button.element.js'),
		},
	},
];

const umbExtensions: Array<ManifestTiptapExtensionButtonKind> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.CodeEditor',
		name: 'Code Editor Tiptap Extension',
		api: () => import('./umb/code-editor.extension.js'),
		weight: 1000,
		meta: {
			alias: 'umb-code-editor',
			icon: 'icon-code',
			label: '#general_viewSourceCode',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embed Tiptap Extension',
		api: () => import('./umb/embed.extension.js'),
		meta: {
			alias: 'umb-embed',
			icon: 'icon-embed',
			label: 'Embed',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./umb/mediapicker.extension.js'),
		meta: {
			alias: 'umb-media',
			icon: 'icon-picture',
			label: 'Media picker',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.UrlPicker',
		name: 'URL Picker Tiptap Extension',
		api: () => import('./umb/urlpicker.extension.js'),
		meta: {
			alias: 'umb-link',
			icon: 'icon-link',
			label: 'URL picker',
		},
	},
];

const extensions: Array<ManifestTiptapExtension> = [...core, ...umbExtensions];

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...kinds, ...extensions];
