import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_LANGUAGE_ENTITY_TYPE, UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		forEntityTypes: [UMB_LANGUAGE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Language.Create',
		name: 'Create Language Entity Action',
		weight: 1200,
		api: () => import('./language-create-entity-action.js'),
		forEntityTypes: [UMB_LANGUAGE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
];
