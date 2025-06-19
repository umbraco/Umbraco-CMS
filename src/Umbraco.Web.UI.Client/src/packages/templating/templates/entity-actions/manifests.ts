import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS } from '../conditions/allow-delete/constants.js';

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
			additionalOptions: true,
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
		conditions: [{ alias: UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS }],
	},
];
