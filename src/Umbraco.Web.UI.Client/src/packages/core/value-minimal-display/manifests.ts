export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.ValueMinimalDisplayCoordinator',
		name: 'Value Minimal Display Coordinator Workspace Context',
		api: () => import('./coordinator/value-minimal-display-coordinator.context.js'),
	},
];
