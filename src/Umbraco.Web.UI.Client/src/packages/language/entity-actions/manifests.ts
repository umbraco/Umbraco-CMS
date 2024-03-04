import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_LANGUAGE_ENTITY_TYPE, UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbLanguageCreateEntityAction } from './language-create-entity-action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		kind: 'delete',
		forEntityTypes: [UMB_LANGUAGE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Create',
		name: 'Create Language Entity Action',
		weight: 900,
		api: UmbLanguageCreateEntityAction,
		forEntityTypes: [UMB_LANGUAGE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions];
