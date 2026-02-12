import {
	UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS,
	UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION,
} from '@umbraco-cms/backoffice/content-type';

const workspaceViewAlias: UmbExtensionManifest = {
	type: 'workspaceView',
	alias: 'Example.WorkspaceView.EntityContentTypeAliasCondition',
	name: 'Example Workspace View With Entity Content Type Alias Condition',
	element: () => import('./content-type-alias-condition-workspace-view.element.js'),
	meta: {
		icon: 'icon-bus',
		label: 'Conditional (Alias)',
		pathname: 'conditional-alias',
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS,
			//match: 'blogPost',
			oneOf: ['blogPost', 'mediaType1'],
		},
	],
};

const workspaceViewUnique: UmbExtensionManifest = {
	type: 'workspaceView',
	alias: 'Example.WorkspaceView.EntityContentTypeUniqueCondition',
	name: 'Example Workspace View With Content Type Unique Condition',
	element: () => import('./content-type-unique-condition-workspace-view.element.js'),
	meta: {
		icon: 'icon-science',
		label: 'Conditional (Unique)',
		pathname: 'conditional-unique',
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION,
			oneOf: ['721e85d3-0a2d-4f99-be55-61a5c5ed5c14', '1b88975d-60d0-4b84-809a-4a4deff38a66'], // Example uniques
		},
	],
};

export const manifests = [workspaceViewAlias, workspaceViewUnique];
