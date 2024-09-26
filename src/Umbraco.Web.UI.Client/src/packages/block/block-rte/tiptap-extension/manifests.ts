import type { ManifestTiptapExtensionButtonKind } from '@umbraco-cms/backoffice/tiptap';

export const manifests: ManifestTiptapExtensionButtonKind[] = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.TiptapExtension.BlockPicker',
		name: 'Block Picker Tiptap Extension Button',
		api: () => import('./block-picker.extension.js'),
		meta: {
			alias: 'umbblockpicker',
			icon: 'icon-plugin',
			label: '#blockEditor_insertBlock',
		},
	},
];
