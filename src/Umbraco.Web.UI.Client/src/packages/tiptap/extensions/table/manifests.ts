import { UMB_TIPTAP_TABLE_PROPERTIES_MODAL_ALIAS } from './modals/constants.js';

const UMB_MENU_TIPTAP_TABLE_ALIAS = 'Umb.Menu.Tiptap.Table';
const UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS = 'Umb.Menu.Tiptap.TableColumn';
const UMB_MENU_TIPTAP_TABLE_ROW_ALIAS = 'Umb.Menu.Tiptap.TableRow';
const UMB_MENU_TIPTAP_TABLE_CELL_ALIAS = 'Umb.Menu.Tiptap.TableCell';

const modals: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_TIPTAP_TABLE_PROPERTIES_MODAL_ALIAS,
		name: 'Tiptap Table Properties Modal',
		element: () => import('./modals/table-properties-modal.element.js'),
	},
];

const coreExtensions: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./table.tiptap-api.js'),
		meta: {
			icon: 'icon-table',
			label: 'Table',
			group: '#tiptap_extGroup_interactive',
		},
	},
];

const toolbarExtensions: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'menu',
		alias: 'Umb.Tiptap.Toolbar.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./table.tiptap-toolbar-api.js'),
		element: () => import('./components/table-toolbar-menu.element.js'),
		forExtensions: ['Umb.Tiptap.Table'],
		items: [
			{
				label: 'Cell',
				menu: UMB_MENU_TIPTAP_TABLE_CELL_ALIAS,
			},
			{
				label: 'Row',
				menu: UMB_MENU_TIPTAP_TABLE_ROW_ALIAS,
			},
			{
				label: 'Column',
				menu: UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS,
				separatorAfter: true,
			},
		],
		menu: UMB_MENU_TIPTAP_TABLE_ALIAS,
		meta: {
			alias: 'table',
			icon: 'icon-table',
			label: 'Table',
			look: 'icon',
		},
	},
];

const tableMenu: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_MENU_TIPTAP_TABLE_ALIAS,
		name: 'Tiptap Table Menu',
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableProperties',
		name: 'Tiptap Table Menu Item: Table Properties',
		api: () => import('./actions/table-properties.action.js'),
		weight: 110,
		meta: {
			label: 'Table properties',
			menus: [UMB_MENU_TIPTAP_TABLE_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableDelete',
		name: 'Tiptap Table Menu Item: Delete Table',
		api: () => import('./actions/table-delete.action.js'),
		weight: 100,
		meta: {
			label: 'Delete table',
			icon: 'icon-trash',
			menus: [UMB_MENU_TIPTAP_TABLE_ALIAS],
		},
	},
];

const tableColumnMenu: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS,
		name: 'Tiptap Table Column Menu',
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableColumnAddBefore',
		name: 'Tiptap Table Menu Item: Add Column Before',
		api: () => import('./actions/table-column-add-before.action.js'),
		weight: 120,
		meta: {
			label: 'Add column before',
			icon: 'icon-navigation-first',
			menus: [UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableColumnAddAfter',
		name: 'Tiptap Table Menu Item: Add Column After',
		api: () => import('./actions/table-column-add-after.action.js'),
		weight: 110,
		meta: {
			label: 'Add column after',
			icon: 'icon-tab-key',
			menus: [UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableColumnDelete',
		name: 'Tiptap Table Menu Item: Delete Column',
		api: () => import('./actions/table-column-delete.action.js'),
		weight: 100,
		meta: {
			label: 'Delete column',
			icon: 'icon-trash',
			menus: [UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableColumnToggleHeader',
		name: 'Tiptap Table Menu Item: Toggle Header Column',
		api: () => import('./actions/table-column-toggle-header.action.js'),
		weight: 90,
		meta: {
			label: 'Toggle header column',
			menus: [UMB_MENU_TIPTAP_TABLE_COLUMN_ALIAS],
		},
	},
];

const tableRowMenu: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_MENU_TIPTAP_TABLE_ROW_ALIAS,
		name: 'Tiptap Table Row Menu',
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableRowAddBefore',
		name: 'Tiptap Table Menu Item: Add Row Before',
		api: () => import('./actions/table-row-add-before.action.js'),
		weight: 120,
		meta: {
			label: 'Add row before',
			icon: 'icon-page-up',
			menus: [UMB_MENU_TIPTAP_TABLE_ROW_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableRowAddAfter',
		name: 'Tiptap Table Menu Item: Add Row After',
		api: () => import('./actions/table-row-add-after.action.js'),
		weight: 110,
		meta: {
			label: 'Add row after',
			icon: 'icon-page-down',
			menus: [UMB_MENU_TIPTAP_TABLE_ROW_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableRowDelete',
		name: 'Tiptap Table Menu Item: Delete Row',
		api: () => import('./actions/table-row-delete.action.js'),
		weight: 100,
		meta: {
			label: 'Delete row',
			icon: 'icon-trash',
			menus: [UMB_MENU_TIPTAP_TABLE_ROW_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableRowToggleHeader',
		name: 'Tiptap Table Menu Item: Toggle Header Row',
		api: () => import('./actions/table-row-toggle-header.action.js'),
		weight: 90,
		meta: {
			label: 'Toggle header row',
			menus: [UMB_MENU_TIPTAP_TABLE_ROW_ALIAS],
		},
	},
];

const tableCellMenu: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_MENU_TIPTAP_TABLE_CELL_ALIAS,
		name: 'Tiptap Table Cell Menu',
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableCellMerge',
		name: 'Tiptap Table Menu Item: Merge Cell',
		api: () => import('./actions/table-cell-merge.action.js'),
		weight: 120,
		meta: {
			label: 'Merge cells',
			menus: [UMB_MENU_TIPTAP_TABLE_CELL_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableCellSplit',
		name: 'Tiptap Table Menu Item: Split Cell',
		api: () => import('./actions/table-cell-split.action.js'),
		weight: 110,
		meta: {
			label: 'Split cell',
			menus: [UMB_MENU_TIPTAP_TABLE_CELL_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableCellMergeSplit',
		name: 'Tiptap Table Menu Item: Merge Or Split Cell',
		api: () => import('./actions/table-cell-merge-split.action.js'),
		weight: 100,
		meta: {
			label: 'Merge or split',
			menus: [UMB_MENU_TIPTAP_TABLE_CELL_ALIAS],
		},
	},
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Umb.MenuItem.Tiptap.TableCellToggleHeader',
		name: 'Tiptap Table Menu Item: Toggle Header Cell',
		api: () => import('./actions/table-cell-toggle-header.action.js'),
		weight: 90,
		meta: {
			label: 'Toggle header cell',
			menus: [UMB_MENU_TIPTAP_TABLE_CELL_ALIAS],
		},
	},
];

const menus: Array<UmbExtensionManifest> = [...tableMenu, ...tableColumnMenu, ...tableRowMenu, ...tableCellMenu];

export const manifests = [...modals, ...coreExtensions, ...toolbarExtensions, ...menus];
