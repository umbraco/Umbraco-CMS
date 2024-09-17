import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/workspace';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION } from '@umbraco-cms/backoffice/content';

export const UMB_MEMBER_WORKSPACE_ALIAS = 'Umb.Workspace.Member';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_MEMBER_WORKSPACE_ALIAS,
	name: 'Member Workspace',
	api: () => import('./member-workspace.context.js'),
	meta: {
		entityType: UMB_MEMBER_ENTITY_TYPE,
	},
};

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Member.Save',
		name: 'Save Member Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.Member.Content',
		name: 'Member Workspace Content View',
		weight: 100,
		meta: {
			label: '#general_details',
			pathname: 'content',
			icon: 'icon-document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Member.Member',
		name: 'Member Workspace Member View',
		js: () => import('./views/member/member-workspace-view-member.element.js'),
		weight: 200,
		meta: {
			label: '#treeHeaders_member',
			pathname: 'member',
			icon: 'icon-user',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests: Array<UmbExtensionManifest> = [workspace, ...workspaceActions, ...workspaceViews];
