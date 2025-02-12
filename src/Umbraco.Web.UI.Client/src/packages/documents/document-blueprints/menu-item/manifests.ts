import { UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../tree/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.DocumentBlueprints',
		name: 'Document Blueprints Menu Item',
		weight: 100,
		meta: {
			treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
			label: '#treeHeaders_contentBlueprints',
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
];
