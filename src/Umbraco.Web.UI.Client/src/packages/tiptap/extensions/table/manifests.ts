import type { ManifestTiptapExtension } from '../types.js';

const coreExtensions: Array<ManifestTiptapExtension> = [
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
		meta: {
			alias: 'table',
			icon: 'icon-table',
			label: 'Table',
			look: 'icon',
			items: [
				{
					label: 'Cell',
					items: [
						{ label: 'Merge cells', data: 'mergeCells' },
						{ label: 'Split cell', data: 'splitCell' },
						{ label: 'Merge or split', data: 'mergeOrSplit' },
						{ label: 'Toggle header cell', data: 'toggleHeaderCell' },
					],
				},
				{
					label: 'Row',
					items: [
						{ label: 'Add row before', icon: 'icon-page-up', data: 'addRowBefore' },
						{ label: 'Add row after', icon: 'icon-page-down', data: 'addRowAfter' },
						{ label: 'Delete row', icon: 'icon-trash', data: 'deleteRow' },
						{ label: 'Toggle header row', data: 'toggleHeaderRow' },
					],
				},
				{
					label: 'Column',
					items: [
						{ label: 'Add column before', icon: 'icon-navigation-first', data: 'addColumnBefore' },
						{ label: 'Add column after', icon: 'icon-tab-key', data: 'addColumnAfter' },
						{ label: 'Delete column', icon: 'icon-trash', data: 'deleteColumn' },
						{ label: 'Toggle header column', data: 'toggleHeaderColumn' },
					],
					separatorAfter: true,
				},
				{ label: 'Delete table', icon: 'icon-trash', data: 'deleteTable' },
			],
		},
	},
];

export const manifests = [...coreExtensions, ...toolbarExtensions];
