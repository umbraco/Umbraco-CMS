import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../../collection/constants.js';
import { UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH } from '../../../user-permissions/document/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'default',
		alias: 'Umb.EntityBulkAction.Document.Unpublish',
		name: 'Unpublish Document Entity Bulk Action',
		weight: 40,
		api: () => import('./unpublish.bulk-action.js'),
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
