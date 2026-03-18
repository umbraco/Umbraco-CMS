import { manifests as booleanManifests } from './boolean/manifests.js';
import { UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_WORKSPACE_CONTEXT_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		alias: UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_WORKSPACE_CONTEXT_ALIAS,
		name: 'Value Minimal Display Coordinator Workspace Context',
		api: () => import('./coordinator/value-minimal-display-coordinator.context.js'),
	},
	...booleanManifests,
];
