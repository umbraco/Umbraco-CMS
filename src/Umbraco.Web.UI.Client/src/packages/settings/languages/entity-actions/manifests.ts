import { LANGUAGE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { LANGUAGE_ENTITY_TYPE, LANGUAGE_ROOT_ENTITY_TYPE } from '../index.js';
import { UmbLanguageCreateEntityAction } from './language-create-entity-action.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			icon: 'umb:trash',
			label: 'Delete',
			entityTypes: [LANGUAGE_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Create',
		name: 'Create Language Entity Action',
		weight: 900,
		api: UmbLanguageCreateEntityAction,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			entityTypes: [LANGUAGE_ENTITY_TYPE, LANGUAGE_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
