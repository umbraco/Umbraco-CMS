import { UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/content-type';
import { UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from 'src/packages/core/workspace/conditions/workspace-content-type-unique/constants.js';

const workspaceViewAlias: UmbExtensionManifest = {
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
			//match: 'blogPost',
			oneOf: ['blogPost', 'mediaType1'],
		},
	],
};

const workspaceViewUnique: UmbExtensionManifest = {
	type: 'workspaceView',
	alias: 'Example.WorkspaceView.EntityContentTypeConditionUnique',
	name: 'Example Workspace View With Content Type Unique Condition',
	element: () => import('./workspace-view-unique.element.js'),
	meta: {
		icon: 'icon-science',
		label: 'Conditional (Unique)',
		pathname: 'conditional-unique',
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
			oneOf: ['9cf7f9ba-6bad-469a-8d8a-524546bb6c82'], // Example GUID
		},
	],
};

export const manifests = [workspaceViewAlias, workspaceViewUnique];
