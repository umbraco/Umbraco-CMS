import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS } from '../repository/index.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.MemberType';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
		name: 'Member Type Workspace',
		api: () => import('./member-type-workspace.context.js'),
		meta: {
			entityType: 'member-type',
		},
	},
	{
		type: 'workspaceView',
		kind: 'contentTypeDesignEditor',
		alias: 'Umb.WorkspaceView.MemberType.Design',
		name: 'Member Type Workspace Design View',
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-member-dashed-line',
			compositionRepositoryAlias: UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.MemberType.Save',
		name: 'Save Member Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
