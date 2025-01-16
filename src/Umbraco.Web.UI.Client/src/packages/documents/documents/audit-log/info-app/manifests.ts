export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document History Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.History',
		element: () => import('./document-history-workspace-info-app.element.js'),
		weight: 90,
	},
];
