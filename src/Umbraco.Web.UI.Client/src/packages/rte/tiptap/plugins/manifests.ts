import type { ManifestTiptapExtension } from '../extensions/tiptap-extension.js';
import type { ManifestTiptapToolbarExtensionButtonKind } from '../extensions/tiptap-toolbar-extension.js';

export const manifests: Array<ManifestTiptapExtension | ManifestTiptapToolbarExtensionButtonKind> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Block',
		name: 'Block Tiptap Extension',
		api: () => import('./block.extension.js'),
		meta: {
			icon: 'icon-block',
			label: 'Block',
			group: 'Interactive Elements',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.BlockPicker',
		name: 'Block Picker Tiptap Extension Button',
		api: () => import('./block-picker-toolbar.extension.js'),
		weight: 90,
		meta: {
			alias: 'umbblockpicker',
			icon: 'icon-plugin',
			label: '#blockEditor_insertBlock',
		},
	},
];
