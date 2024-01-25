import { UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE, UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentRecycleBin.Tree.ReloadTreeItemChildren',
		name: 'Reload Document Recycle Bin Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.DocumentRecycleBin.Tree',
			entityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE, UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		},
	},
];
