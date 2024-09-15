export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.RelationTypeRoot',
		name: 'Relation Type Root Workspace',
		element: () => import('./relation-type-root-workspace.element.js'),
		meta: {
			entityType: 'relation-type-root',
		},
	},
];
