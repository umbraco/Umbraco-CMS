import { UMB_MEMBER_GROUP_ENTITY_TYPE, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbReloadTreeItemChildrenEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberGroup.Tree.ReloadTreeItemChildren',
		name: 'Reload Member Group Tree Item Children Entity Action',
		weight: 10,
		api: UmbReloadTreeItemChildrenEntityAction,
		meta: {
			icon: 'icon-refresh',
			label: 'Reload children...',
			repositoryAlias: 'Umb.Repository.MemberGroup.Tree',
			entityTypes: [UMB_MEMBER_GROUP_ENTITY_TYPE, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE],
		},
	},
];
