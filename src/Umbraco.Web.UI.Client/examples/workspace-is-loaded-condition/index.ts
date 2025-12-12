export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Example.WorkspaceAction.IsLoadedTest',
		name: 'Is Loaded Test Action',
		weight: 9999,
		api: () => import('./is-loaded-test.action.js'),
		meta: {
			label: 'Test Is Loaded',
			look: 'primary',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
			{
				alias: 'Umb.Condition.Workspace.EntityDetailIsLoaded',
			},
		],
	},
];
