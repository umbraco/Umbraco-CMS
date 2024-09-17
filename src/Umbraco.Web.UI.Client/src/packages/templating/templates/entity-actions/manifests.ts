import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		weight: 1200,
		api: () => import('./create/create.action.js'),
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Template.Delete',
		name: 'Delete Template Entity Action',
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS,
		},
	},
];
