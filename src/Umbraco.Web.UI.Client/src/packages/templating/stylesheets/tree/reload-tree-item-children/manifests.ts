import {
	UMB_STYLESHEET_ROOT_ENTITY_TYPE,
	UMB_STYLESHEET_ENTITY_TYPE,
	UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
} from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Tree.ReloadTreeItemChildren',
		name: 'Reload Stylesheet Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.Stylesheet.Tree',
			entityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		},
	},
];
