import { UmbReloadTreeItemChildrenEntityAction } from './reload-tree-item-children.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_GROUP_TREE } from '@umbraco-cms/backoffice/action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Tree.ReloadChildrenOf',
	matchKind: 'reloadTreeItemChildren',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		group: UMB_ACTION_GROUP_TREE,
		api: UmbReloadTreeItemChildrenEntityAction,
		weight: 0,
		forEntityTypes: [],
		meta: {
			icon: 'icon-refresh',
			label: '#actions_refreshNode',
		},
	},
};
