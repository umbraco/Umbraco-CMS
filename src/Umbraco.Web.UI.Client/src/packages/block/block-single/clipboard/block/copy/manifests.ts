import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.BlockSingleToBlock',
		name: 'Block Single To Block Clipboard Copy Property Value Translator',
		api: () => import('./block-single-to-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
];
