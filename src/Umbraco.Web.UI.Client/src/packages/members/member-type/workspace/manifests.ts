import { UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../entity.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UmbSchemaLockdownInfoAppElement,
	UmbSchemaLockdownWorkspaceActionApi,
	UmbSchemaLockdownWorkspaceActionElement,
	UmbSubmitWorkspaceAction,
} from '@umbraco-cms/backoffice/workspace';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

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
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
				operation: 'update',
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MemberType.SchemaLockdown',
		name: 'Member Type Schema Lockdown Workspace Action',
		api: UmbSchemaLockdownWorkspaceActionApi,
		element: UmbSchemaLockdownWorkspaceActionElement,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
				operation: 'update',
				match: false,
			},
		],
	},
	{
		type: 'workspaceInfoApp',
		alias: 'Umb.WorkspaceInfoApp.MemberType.SchemaLockdown',
		name: 'Member Type Schema Lockdown Notice',
		element: UmbSchemaLockdownInfoAppElement,
		weight: 1000,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
				operation: 'update',
				match: false,
			},
		],
	},
];
