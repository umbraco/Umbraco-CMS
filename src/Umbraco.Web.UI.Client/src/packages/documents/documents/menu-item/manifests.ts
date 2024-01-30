import { UMB_CONTENT_MENU_ALIAS } from '../menu.manifests.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Document',
	name: 'Document Menu Item',
	weight: 200,
	meta: {
		label: 'Documents',
		menus: [UMB_CONTENT_MENU_ALIAS],
		treeAlias: UMB_DOCUMENT_TREE_ALIAS,
		hideTreeRoot: true,
	},
};

export const manifests = [menuItem];
