import { UmbTiptapBlockRteToBlockClipboardCopyPropertyValueTranslator } from './block-rte-to-block-copy-translator.js';
import { UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/tiptap-rte/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifest: UmbExtensionManifest = {
	type: 'clipboardCopyPropertyValueTranslator',
	alias: 'Umb.ClipboardCopyPropertyValueTranslator.Tiptap.BlockRteToBlock',
	name: 'Tiptap Block RTE To Block Clipboard Copy Property Value Translator',
	api: UmbTiptapBlockRteToBlockClipboardCopyPropertyValueTranslator,
	fromPropertyEditorUi: UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS,
	toClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
};
