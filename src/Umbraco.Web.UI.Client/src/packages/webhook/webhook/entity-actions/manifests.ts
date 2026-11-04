import { UMB_WEBHOOK_ENTITY_TYPE } from '../../entity.js';
import { UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS, UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Webhook.Delete',
		name: 'Delete Webhook Entity Action',
		forEntityTypes: [UMB_WEBHOOK_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_WEBHOOK_ENTITY_TYPE,
				operation: 'delete',
			},
		],
	},
];
