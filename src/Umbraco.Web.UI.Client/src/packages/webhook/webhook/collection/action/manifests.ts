import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Webhook Collection Action',
		alias: 'Umb.CollectionAction.Webhook.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: 'section/settings/workspace/webhook/create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Webhook',
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_WEBHOOK_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
];
