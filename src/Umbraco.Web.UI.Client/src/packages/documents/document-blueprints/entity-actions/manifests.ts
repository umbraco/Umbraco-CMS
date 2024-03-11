/*import {
	UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
} from '../repository/index.js';*/
import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ITEM_ENTITY_TYPE,
} from '../entity.js';
import { UmbCreateEntityAction } from './create/create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DocumentBlueprint.Create',
		name: 'Create Document Blueprint Entity Action',
		api: UmbCreateEntityAction,
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Create',
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DocumentBlueprintItem.Create',
		name: 'Create Document Blueprint Item Entity Action',
		api: UmbCreateEntityAction,
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Create',
		},
	} /*
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DocumentBlueprintItem.Delete',
		name: 'Delete Document Blueprint Item Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ITEM_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
		},
	},
	*/,
];

export const manifests = [...entityActions];
