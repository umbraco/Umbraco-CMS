import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/index.js';
import { manifests as structureManifests } from './structure/manifests.js';
import type { ManifestMenuItemTreeKind, ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CONTENT_MENU_ALIAS = 'Umb.Menu.Content';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_CONTENT_MENU_ALIAS,
	name: 'Content Menu',
	meta: {
		label: 'Content',
	},
};

const menuItem: ManifestMenuItemTreeKind = {
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

export const manifests = [menu, menuItem, ...structureManifests];
