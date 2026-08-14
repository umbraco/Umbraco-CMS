import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...menuItemManifests, ...workspaceManifests];

export const name = 'Umbraco.Core.ExtensionInsight';
export const extensions = [
	{
		name: 'Extension Insight Bundle',
		alias: 'Umb.Bundle.ExtensionInsight',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
