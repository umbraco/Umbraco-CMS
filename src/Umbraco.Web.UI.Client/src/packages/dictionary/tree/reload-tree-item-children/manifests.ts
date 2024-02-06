import { UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Tree.ReloadTreeItemChildren',
		name: 'Reload Dictionary Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE],
		},
	},
];
