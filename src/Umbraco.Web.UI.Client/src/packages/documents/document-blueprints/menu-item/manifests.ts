import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../entity.js';
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
		entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
