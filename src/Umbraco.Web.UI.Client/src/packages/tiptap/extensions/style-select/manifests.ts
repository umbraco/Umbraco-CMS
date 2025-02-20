import type { ManifestTiptapToolbarExtension } from '../types.js';

export const manifests: Array<ManifestTiptapToolbarExtension> = [
	{
		type: 'tiptapToolbarExtension',
		alias: 'Umb.Tiptap.Toolbar.StyleSelect',
		name: 'Style Select Tiptap Extension',
		element: () => import('./style-select-tiptap-toolbar.element.js'),
		meta: {
			alias: 'umbStyleSelect',
			icon: 'icon-palette',
			label: 'Style Select',
		},
	},
];
