import { UMB_DICTIONARY_ENTITY_TYPE } from '../entities.js';
import { UMB_DICTIONARY_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Dictionary',
	name: 'Dictionary Menu Item',
	weight: 400,
	meta: {
		label: 'Dictionary',
		icon: 'icon-book-alt',
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		treeAlias: UMB_DICTIONARY_TREE_ALIAS,
		menus: ['Umb.Menu.Dictionary'],
	},
};

export const manifests = [menuItem];
