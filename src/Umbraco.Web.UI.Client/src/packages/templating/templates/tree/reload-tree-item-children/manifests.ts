import {
	UMB_TEMPLATE_ROOT_ENTITY_TYPE,
	UMB_TEMPLATE_ENTITY_TYPE,
	UMB_TEMPLATE_FOLDER_ENTITY_TYPE,
} from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Tree.ReloadTreeItemChildren',
		name: 'Reload Template Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.Template.Tree',
			entityTypes: [UMB_TEMPLATE_ROOT_ENTITY_TYPE, UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_FOLDER_ENTITY_TYPE],
		},
	},
];
