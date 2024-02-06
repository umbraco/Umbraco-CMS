import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
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

const workspaceViews: Array<ManifestWorkspaceView> = [
	/*
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.MemberGroup.Info',
		name: 'Member Group Workspace Info View',
		js: () => import('./views/info/workspace-view-member-group-info.element.js'),
		weight: 90,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	*/
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	/*
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
				match: workspace.alias,
			},
		],
	},
	*/
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
