export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document Links Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.Links',
		element: () => import('./document-links-workspace-info-app.element.js'),
		weight: 100,
	},
];
