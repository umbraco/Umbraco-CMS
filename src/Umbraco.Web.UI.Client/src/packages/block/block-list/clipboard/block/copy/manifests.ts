import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/constants.js';
import { UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.BlockListToBlock',
		name: 'Block List To Block Clipboard Copy Translator',
		api: () => import('./block-list-to-block-copy-translator.js'),
		fromPropertyEditorUi: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: UMB_BLOCK_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
];
