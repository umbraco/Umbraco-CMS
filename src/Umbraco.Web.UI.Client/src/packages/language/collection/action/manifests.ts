import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Language Collection Action',
		alias: 'Umb.CollectionAction.Language.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: 'section/settings/workspace/language/create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Language',
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_LANGUAGE_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
];
