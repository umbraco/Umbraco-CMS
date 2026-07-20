import { manifests as previewApps } from './preview-apps/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...previewApps];

export const name = 'Umbraco.Core.Preview';
export const extensions = [
	{
		name: 'Preview Bundle',
		alias: 'Umb.Bundle.Preview',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
