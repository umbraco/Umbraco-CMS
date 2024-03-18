export const manifests = [
	{
		type: 'workspaceContext',
		name: 'Document Structure Context',
		alias: 'Umb.Context.Document.Structure',
		api: () => import('./document-navigation-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
