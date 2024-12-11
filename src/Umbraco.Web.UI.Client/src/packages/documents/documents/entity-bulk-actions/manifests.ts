import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
} from '../user-permissions/constants.js';
import { manifests as duplicateToManifests } from './duplicate-to/manifests.js';
import { manifests as moveToManifests } from './move-to/manifests.js';
import { manifests as trashManifests } from './trash/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

export const entityBulkActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Publish',
		name: 'Publish Document Entity Bulk Action',
		weight: 50,
		api: () => import('./publish/publish.action.js'),
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
		],
	},
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Unpublish',
		name: 'Unpublish Document Entity Bulk Action',
		weight: 40,
		api: () => import('./unpublish/unpublish.action.js'),
		meta: {
			icon: 'icon-globe',
			label: '#actions_unpublish',
		},
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH],
			},
		],
	},
];

export const manifests: Array<UmbExtensionManifest> = [
	...entityBulkActions,
	...duplicateToManifests,
	...moveToManifests,
	...trashManifests,
];
