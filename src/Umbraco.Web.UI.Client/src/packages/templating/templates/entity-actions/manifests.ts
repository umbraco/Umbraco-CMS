import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbCreateEntityAction } from './create/create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		api: UmbCreateEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Delete',
		name: 'Delete Template Entity Action',
		kind: 'delete',
		meta: {
			detailRepositoryAlias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_TEMPLATE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
