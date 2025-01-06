import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Copy',
		name: 'Block Grid Copy To Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.BlockGridToGridBlock',
		name: 'Block Grid To Grid Block Clipboard Copy Translator',
		api: () => import('./block-grid-to-grid-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: 'gridBlock',
	},
	{
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.BlockGridToBlock',
		name: 'Block Grid to Block Clipboard Copy Translator',
		api: () => import('./block-grid-to-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
];
