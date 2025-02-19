import type { ManifestTiptapExtension, ManifestTiptapToolbarExtension } from '../types.js';

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

const toolbarExtensions: Array<ManifestTiptapToolbarExtension> = [
	{
		type: 'tiptapToolbarExtension',
		alias: 'Umb.Tiptap.Toolbar.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./table.tiptap-toolbar-api.js'),
		element: () => import('./table-tiptap-toolbar-button.element.js'),
		forExtensions: ['Umb.Tiptap.Table'],
		meta: {
			alias: 'table',
			icon: 'icon-table',
			label: 'Table',
		},
	},
];

export const manifests = [...coreExtensions, ...toolbarExtensions];
