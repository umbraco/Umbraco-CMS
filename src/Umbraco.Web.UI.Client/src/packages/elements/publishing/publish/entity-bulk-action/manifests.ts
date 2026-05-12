import { UMB_ELEMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_ELEMENT_COLLECTION_ALIAS } from '../../../collection/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_PUBLISH,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../../../user-permissions/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Element.Publish',
		name: 'Publish Element Entity Bulk Action',
		weight: 50,
		api: () => import('./publish.bulk-action.js'),
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
		},
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ELEMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_UPDATE, UMB_USER_PERMISSION_ELEMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
