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

const umbExtensions: Array<ManifestTiptapExtension | ManifestTiptapExtensionButtonKind> = [
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
		alias: 'Umb.Tiptap.MediaUpload',
		name: 'Media Upload Tiptap Extension',
		api: () => import('./umb/media-upload.extension.js'),
		weight: 900,
		meta: {
			alias: 'umbMediaUpload',
			icon: 'icon-image-up',
			label: 'Media upload',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./umb/link.extension.js'),
		weight: 102,
		meta: {
			alias: 'umbLink',
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./umb/mediapicker.extension.js'),
		weight: 80,
		meta: {
			alias: 'umbMedia',
			icon: 'icon-picture',
			label: 'Media picker',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embed Tiptap Extension',
		api: () => import('./umb/embedded-media.extension.js'),
		weight: 70,
		meta: {
			alias: 'umbEmbeddedMedia',
			icon: 'icon-embed',
			label: '#general_embed',
		},
	},
];

const extensions: Array<ManifestTiptapExtension> = [...core, ...umbExtensions];

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...kinds, ...extensions];
