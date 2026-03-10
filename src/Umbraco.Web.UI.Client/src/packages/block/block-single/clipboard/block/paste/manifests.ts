import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.BlockToBlockSingle',
		name: 'Block To Block Single Clipboard Paste Property Value Translator',
		api: () => import('./block-to-block-single-paste-translator.js'),
		fromClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS,
	},
];
