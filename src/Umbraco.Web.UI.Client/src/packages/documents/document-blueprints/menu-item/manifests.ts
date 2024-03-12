import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DocumentBlueprints',
	name: 'Document Blueprints Menu Item',
	weight: 100,
	meta: {
		treeAlias: 'Umb.Tree.DocumentBlueprint',
		label: 'Document Blueprints',
		icon: 'icon-blueprint',
		entityType: 'document-blueprint-root',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
