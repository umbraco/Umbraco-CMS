import { UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

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
			alias: UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS,
			//match : 'blogPost'
			oneOf: ['blogPost', 'mediaType1'],
		},
	],
};

export const manifests = [workspace];
