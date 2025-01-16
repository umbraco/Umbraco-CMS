export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.References',
		element: () => import('./document-references-workspace-view-info.element.js'),
		weight: 90,
	},
];
