import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_TYPE_WORKSPACE_ALIAS } from '../../member-type/workspace/manifests.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_WORKSPACE_ALIAS = 'Umb.Workspace.MemberGroup';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
	name: 'MemberGroup Workspace',
	js: () => import('./member-group-workspace.element.js'),
	meta: {
		entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
	},
};

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MemberGroup.Save',
		name: 'Save Member Group Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Member.Info',
		name: 'Member Workspace info View',
		js: () => import('./views/info/member-type-workspace-view-info.element.js'),
		weight: 300,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'icon-document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceActions, ...workspaceViews];
