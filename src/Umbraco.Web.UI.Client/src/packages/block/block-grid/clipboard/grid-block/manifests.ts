import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.BlockGridToGridBlock',
		name: 'Block Grid To Grid Block Clipboard Copy Translator',
		api: () => import('./copy/block-grid-to-grid-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: 'gridBlock',
	},
	{
		type: 'clipboardPasteTranslator',
		alias: 'Umb.ClipboardPasteTranslator.GridBlockToBlockGrid',
		name: 'Grid Block To Block Grid Clipboard Paste Translator',
		api: () => import('./paste/grid-block-to-block-grid-paste-translator.js'),
		fromClipboardEntryValueType: 'gridBlock',
		weight: 1000,
		toPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	},
];
