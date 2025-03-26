import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_DELETE } from '../user-permissions/constants.js';
import { UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from '../reference/constants.js';
import { manifests as createBlueprintManifests } from './create-blueprint/manifests.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as sortChildrenOfManifests } from './sort-children-of/manifests.js';
import { manifests as notificationManifests } from './notifications/manifests.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

const entityActions: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'deleteWithRelation',
		alias: 'Umb.EntityAction.Document.Delete',
		name: 'Delete Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
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
];

export const manifests: Array<UmbExtensionManifest> = [
	...createBlueprintManifests,
	...createManifests,
	...cultureAndHostnamesManifests,
	...duplicateManifests,
	...moveManifests,
	...sortChildrenOfManifests,
	...entityActions,
	...notificationManifests,
];
