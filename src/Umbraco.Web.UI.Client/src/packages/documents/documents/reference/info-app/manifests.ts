export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.References',
		element: () => import('./document-workspace-view-info-reference.element.js'),
		weight: 90,
	},
];
