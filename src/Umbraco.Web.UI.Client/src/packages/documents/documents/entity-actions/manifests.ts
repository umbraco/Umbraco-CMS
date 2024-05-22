import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
	UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
} from '../user-permissions/constants.js';
import { manifests as createBlueprintManifests } from './create-blueprint/manifests.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as publicAccessManifests } from './public-access/manifests.js';
import { manifests as sortChildrenOfManifests } from './sort-children-of/manifests.js';

import type { ManifestEntityAction, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/recycle-bin';

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
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
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
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
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
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	/* TODO: Implement Permissions Entity Action
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
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	*/
	/* TODO: Implement Notifications Entity Action
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
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	*/
];

export const manifests: Array<ManifestTypes> = [
	...createBlueprintManifests,
	...createManifests,
	...cultureAndHostnamesManifests,
	...duplicateManifests,
	...moveManifests,
	...publicAccessManifests,
	...sortChildrenOfManifests,
	...entityActions,
];
