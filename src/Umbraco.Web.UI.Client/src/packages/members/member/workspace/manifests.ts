import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_WORKSPACE_ALIAS = 'Umb.Workspace.Member';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_MEMBER_WORKSPACE_ALIAS,
	name: 'Member Workspace',
	js: () => import('./member-workspace.element.js'),
	meta: {
		entityType: UMB_MEMBER_ENTITY_TYPE,
	},
};

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Member.Save',
		name: 'Save Member Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
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
		alias: 'Umb.WorkspaceView.Member.Content',
		name: 'Member Workspace Content View',
		js: () => import('./views/content/member-workspace-view-content.element.js'),
		weight: 300,
		meta: {
			label: 'Content',
			pathname: 'content',
			icon: 'icon-document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_WORKSPACE_ALIAS,
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
			label: 'Member',
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

export const manifests = [workspace, ...workspaceActions, ...workspaceViews];
