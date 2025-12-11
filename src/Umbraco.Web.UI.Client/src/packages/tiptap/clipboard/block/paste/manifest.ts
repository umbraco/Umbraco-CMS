import { UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/tiptap-rte/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifest: UmbExtensionManifest = {
	type: 'clipboardPastePropertyValueTranslator',
	alias: 'Umb.ClipboardPastePropertyValueTranslator.Tiptap.BlockToBlockRte',
	name: 'Tiptap Block To Block RTE Clipboard Paste Property Value Translator',
	api: () => import('./block-to-block-rte-paste-translator.js'),
	fromClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	toPropertyEditorUi: UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS,
};
