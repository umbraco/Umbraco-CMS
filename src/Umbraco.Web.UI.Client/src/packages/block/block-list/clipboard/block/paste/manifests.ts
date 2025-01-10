import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../../../property-editors/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.BlockToBlockList',
		name: 'Block To Block List Clipboard Paste Property Value Translator',
		api: () => import('./block-to-block-list-paste-translator.js'),
		fromClipboardEntryValueType: 'block',
		toPropertyEditorUi: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
	},
];
