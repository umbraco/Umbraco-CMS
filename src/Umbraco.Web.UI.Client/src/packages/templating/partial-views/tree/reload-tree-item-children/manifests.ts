import {
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
} from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Tree.ReloadTreeItemChildren',
		name: 'Reload Partial View Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.PartialView.Tree',
			entityTypes: [
				UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
				UMB_PARTIAL_VIEW_ENTITY_TYPE,
				UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
			],
		},
	},
];
