import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.BlockGridToBlock',
		name: 'Block Grid to Block Clipboard Copy Translator',
		api: () => import('./copy/block-grid-to-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
	{
		type: 'clipboardPasteTranslator',
		alias: 'Umb.ClipboardPasteTranslator.BlockToBlockGrid',
		name: 'Block To Block Grid Clipboard Paste Translator',
		weight: 900,
		api: () => import('./paste/block-to-block-grid-paste-translator.js'),
		fromClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	},
];
