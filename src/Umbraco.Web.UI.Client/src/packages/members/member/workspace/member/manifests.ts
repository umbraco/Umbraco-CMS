import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_MEMBER_WORKSPACE_ALIAS,
	UMB_MEMBER_WORKSPACE_VIEW_CONTENT_ALIAS,
	UMB_MEMBER_WORKSPACE_VIEW_MEMBER_ALIAS,
} from './constants.js';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/workspace';
import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION } from '@umbraco-cms/backoffice/content';

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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: UMB_MEMBER_WORKSPACE_VIEW_CONTENT_ALIAS,
		name: 'Member Workspace Content View',
		weight: 1000,
		meta: {
			label: '#general_details',
			pathname: 'content',
			icon: 'icon-document',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: UMB_MEMBER_WORKSPACE_VIEW_MEMBER_ALIAS,
		name: 'Member Workspace Member View',
		js: () => import('./views/member/member-workspace-view-member.element.js'),
		weight: 500,
		meta: {
			label: '#apps_umbInfo',
			pathname: 'info',
			icon: 'icon-user',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests: Array<UmbExtensionManifest> = [workspace, ...workspaceActions, ...workspaceViews];
