import { UMB_RELATION_TYPE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.RelationTypes',
	name: 'Relation Types Menu Item',
	weight: 100,
	meta: {
		treeAlias: UMB_RELATION_TYPE_TREE_ALIAS,
		label: 'Relation Types',
		menus: ['Umb.Menu.AdvancedSettings'],
	},
};

export const manifests = [menuItem];
