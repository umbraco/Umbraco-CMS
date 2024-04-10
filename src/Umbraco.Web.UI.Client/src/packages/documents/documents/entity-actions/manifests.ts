import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_PICKER_MODAL } from '../modals/index.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
	UMB_USER_PERMISSION_DOCUMENT_MOVE,
	UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
	UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
} from '../user-permissions/constants.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as publicAccessManifests } from './public-access/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import { manifests as sortChildrenOfManifests } from './sort-children-of/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Document.Delete',
		name: 'Delete Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 1000,
		api: () => import('./create-blueprint.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-blueprint',
			label: '#actions_createblueprint',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT],
			},
		],
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.MoveTo',
		name: 'Move Document Entity Action ',
		kind: 'moveTo',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		weight: 900,
		meta: {
			moveRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModelAlias: UMB_DOCUMENT_PICKER_MODAL,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_MOVE],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.Document.Duplicate',
		name: 'Duplicate Document Entity Action',
		weight: 800,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_PICKER_MODAL,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DUPLICATE],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		weight: 600,
		api: () => import('./publish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		weight: 500,
		api: () => import('./unpublish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: '#actions_unpublish',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Permissions Document Entity Action',
		weight: 300,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		api: () => import('./permissions.action.js'),
		meta: {
			icon: 'icon-name-badge',
			label: '#actions_setPermissions',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Notifications',
		name: 'Notifications Document Entity Action',
		weight: 100,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		api: () => import('./permissions.action.js'),
		meta: {
			icon: 'icon-megaphone',
			label: '#actions_notify',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS],
			},
		],
	},
];

export const manifests = [
	...createManifests,
	...publicAccessManifests,
	...cultureAndHostnamesManifests,
	...sortChildrenOfManifests,
	...entityActions,
];
