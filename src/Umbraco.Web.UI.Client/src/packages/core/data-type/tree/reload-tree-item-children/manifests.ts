import {
	UMB_DATA_TYPE_ENTITY_TYPE,
	UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Tree.ReloadTreeItemChildren',
		name: 'Reload Data Type Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DATA_TYPE_ENTITY_TYPE, UMB_DATA_TYPE_ROOT_ENTITY_TYPE, UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
];
