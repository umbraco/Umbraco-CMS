import { manifests as clipboardManifests } from './clipboard/manifests.js';
import { manifests as tiptapExtensionManifests } from './extensions/manifests.js';
import { manifests as propertyEditors } from './property-editors/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as richTextValueSummary } from './property-editors/tiptap-rte/value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...clipboardManifests,
	...tiptapExtensionManifests,
	...propertyEditors,
	...richTextValueSummary,
];

export const name = 'Umbraco.Core.Tiptap';
export const extensions = [
	{
		name: 'Tiptap Bundle',
		alias: 'Umb.Bundle.Tiptap',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
