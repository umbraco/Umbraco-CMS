import { UMB_CONTENT_MENU_ALIAS } from '../../menu/manifests.js';
import { UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS } from '../tree/index.js';
import type { ManifestMenuItemTreeKind, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Document.RecycleBin',
	name: 'Document Recycle Bin Menu Item',
	weight: 100,
	meta: {
		treeAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
		label: 'Recycle Bin',
		icon: 'icon-trash',
		menus: [UMB_CONTENT_MENU_ALIAS],
	},
	conditions: [
		{
			alias: 'Umb.Condition.CurrentUser.AllowDocumentRecycleBin',
		},
	],
};

export const manifests: Array<ManifestTypes> = [menuItem];
