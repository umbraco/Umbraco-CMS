import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_GRID_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.BlockGridToGridBlock',
		name: 'Block Grid To Grid Block Clipboard Copy Property Value Translator',
		api: () => import('./copy/block-grid-to-grid-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: UMB_GRID_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.GridBlockToBlockGrid',
		name: 'Grid Block To Block Grid Clipboard Paste Property Value Translator',
		api: () => import('./paste/grid-block-to-block-grid-paste-translator.js'),
		fromClipboardEntryValueType: UMB_GRID_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
		weight: 1000,
		toPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	},
];
