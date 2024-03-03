import { UmbReloadTreeItemChildrenEntityAction } from './reload-tree-item-children.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Tree.ReloadChildrenOf',
	matchKind: 'reloadTreeItemChildren',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		api: UmbReloadTreeItemChildrenEntityAction,
		weight: 800,
		meta: {
			entityTypes: [],
		},
	},
};
