import { UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../tree/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DocumentBlueprints',
	name: 'Document Blueprints Menu Item',
	weight: 100,
	meta: {
		treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
		label: 'Document Blueprints',
		icon: 'icon-blueprint',
		entityType: 'document-blueprint-root',
		menus: ['Umb.Menu.StructureSettings'],
	},
};

export const manifests = [menuItem];
