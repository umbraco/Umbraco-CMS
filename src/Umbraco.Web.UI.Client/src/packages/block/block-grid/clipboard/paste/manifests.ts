import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Paste',
		name: 'Block Grid Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'clipboardPasteTranslator',
		alias: 'Umb.ClipboardPasteTranslator.GridBlockToBlockGrid',
		name: 'Grid Block To Block Grid Clipboard Paste Translator',
		api: () => import('./grid-block-to-block-grid-paste-translator.js'),
		fromClipboardEntryValueType: 'gridBlock',
		weight: 1000,
		toPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	},
	{
		type: 'clipboardPasteTranslator',
		alias: 'Umb.ClipboardPasteTranslator.BlockToBlockGrid',
		name: 'Block To Block Grid Clipboard Paste Translator',
		weight: 900,
		api: () => import('./block-to-block-grid-paste-translator.js'),
		fromClipboardEntryValueType: 'block',
		toPropertyEditorUi: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	},
];
