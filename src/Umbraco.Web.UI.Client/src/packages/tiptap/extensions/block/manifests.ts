import type { ManifestTiptapExtension } from '../tiptap.extension.js';
import type { ManifestTiptapToolbarExtensionButtonKind } from '../tiptap-toolbar.extension.js';

export const manifests: Array<ManifestTiptapExtension | ManifestTiptapToolbarExtensionButtonKind> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Block',
		name: 'Block Tiptap Extension',
		api: () => import('./block.tipap-api.js'),
		meta: {
			icon: 'icon-plugin',
			label: 'Block',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.BlockPicker',
		name: 'Block Picker Tiptap Extension Button',
		api: () => import('./block.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Block'],
		meta: {
			alias: 'umbblockpicker',
			icon: 'icon-plugin',
			label: '#blockEditor_insertBlock',
		},
	},
];
