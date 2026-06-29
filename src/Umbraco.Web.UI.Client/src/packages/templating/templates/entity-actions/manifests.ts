import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS } from '../conditions/allow-delete/constants.js';
import { manifests as defaultManifests } from './create/default/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	...defaultManifests,
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
		conditions: [
			{ alias: UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS },
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
