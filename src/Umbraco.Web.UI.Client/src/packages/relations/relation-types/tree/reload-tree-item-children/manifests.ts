import { UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Tree.ReloadTreeItemChildren',
		name: 'Reload Relation Type Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.RelationType.Tree',
			entityTypes: [UMB_RELATION_TYPE_ROOT_ENTITY_TYPE],
		},
	},
];
