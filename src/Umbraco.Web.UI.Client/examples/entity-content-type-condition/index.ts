const workspace: UmbExtensionManifest = {
	type: 'workspaceView',
	alias: 'Example.WorkspaceView.EntityContentTypeCondition',
	name: 'Example Workspace View With Entity Content Type Condition',
	element: () => import('./workspace-view.element.js'),
	meta: {
		icon: 'icon-bus',
		label: 'Conditional',
		pathname: 'conditional',
	},
	conditions: [
		{
			alias: 'Umb.Condition.WorkspaceContentTypeAlias',
			//match : 'blogPost'
			oneOf: ['blogPost', 'mediaType1'],
		},
	],
};

export const manifests = [workspace];
