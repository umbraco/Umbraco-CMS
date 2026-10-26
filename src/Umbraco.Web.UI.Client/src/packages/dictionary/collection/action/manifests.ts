import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN } from '../../workspace/index.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

const createPath = UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN.generateAbsolute({
	parentEntityType: UMB_DICTIONARY_ROOT_ENTITY_TYPE,
	parentUnique: null,
});

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Dictionary Collection Action',
		alias: 'Umb.CollectionAction.Dictionary.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: createPath,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Dictionary',
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_DICTIONARY_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
];
