import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { manifests as core } from './core/manifests.js';
import { manifests as toolbar } from './toolbar/manifests.js';
import type {
	ManifestTiptapToolbarExtension,
	ManifestTiptapToolbarExtensionButtonKind,
} from './tiptap-toolbar-extension.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const kinds: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Button',
		matchKind: 'button',
		matchType: 'tiptapToolbarExtension',
		manifest: {
			element: () => import('../components/toolbar/tiptap-toolbar-button.element.js'),
		},
	},
];

const umbToolbarExtensions: Array<ManifestTiptapToolbarExtension | ManifestTiptapToolbarExtensionButtonKind> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.CodeEditor',
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
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./umb/link.extension.js'),
		weight: 102,
		meta: {
			alias: 'umbLink',
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
			isDefault: true,
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./umb/mediapicker.extension.js'),
		weight: 80,
		meta: {
			alias: 'umbMedia',
			icon: 'icon-picture',
			label: 'Media picker',
			isDefault: true,
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Embed',
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

const umbExtensions: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaUpload',
		name: 'Media Upload Tiptap Extension',
		api: () => import('./umb/media-upload.extension.js'),
		meta: {
			icon: 'icon-image-up',
			label: 'Media upload',
			group: 'Media and Embeds',
		},
	},
];

const extensions = [...core, ...toolbar, ...umbToolbarExtensions, ...umbExtensions];

export const manifests = [...kinds, ...extensions];
