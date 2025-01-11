import { UMB_TINY_MCE_PROPERTY_EDITOR_UI_ALIAS } from '../../../constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.BlockToTinyMce',
		name: 'Block To Tiny MCE Clipboard Paste Property Value Translator',
		api: () => import('./block-to-tiny-mce-paste-translator.js'),
		fromClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: UMB_TINY_MCE_PROPERTY_EDITOR_UI_ALIAS,
	},
];
