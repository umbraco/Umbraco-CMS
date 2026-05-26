import { manifests as treeManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...treeManifests, ...workspaceManifests];

export const name = 'Umbraco.Core.LogViewer';
export const extensions = [
	{
		name: 'Log Viewer Bundle',
		alias: 'Umb.Bundle.LogViewer',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
