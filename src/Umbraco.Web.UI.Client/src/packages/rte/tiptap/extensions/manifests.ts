import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { manifests as core } from './core/manifests.js';
import { manifest as codeEditor } from './code-editor.extension.js';
import { manifest as embed } from './embed.extension.js';
import { manifest as mediaPicker } from './mediapicker.extension.js';
import { manifest as urlPicker } from './urlpicker.extension.js';
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

const extensions: Array<ManifestTiptapExtension> = [...core, codeEditor, embed, mediaPicker, urlPicker];

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...kinds, ...extensions];
