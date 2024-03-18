export const manifests = [
	{
		type: 'workspaceContext',
		name: 'Script Navigation Structure Workspace Context',
		alias: 'Umb.Context.Script.NavigationStructure',
		api: () => import('./script-navigation-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Script',
			},
		],
	},
];
