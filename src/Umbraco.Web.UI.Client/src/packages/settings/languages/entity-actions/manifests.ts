import { LANGUAGE_ENTITY_TYPE, LANGUAGE_ROOT_ENTITY_TYPE } from '..';
import { LANGUAGE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbLanguageCreateEntityAction } from './language-create-entity-action';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		meta: {
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
		},
		conditions: {
			entityTypes: [LANGUAGE_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Create',
		name: 'Create Language Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			api: UmbLanguageCreateEntityAction,
		},
		conditions: {
			entityTypes: [LANGUAGE_ENTITY_TYPE, LANGUAGE_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
