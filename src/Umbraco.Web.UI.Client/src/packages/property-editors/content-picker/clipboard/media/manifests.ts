import { UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE } from '@umbraco-cms/backoffice/media';

const forPropertyEditorUi = 'Umb.PropertyEditorUi.ContentPicker';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.ContentPickerToMedia',
		name: 'Content Picker To Media Clipboard Copy Property Value Translator',
		api: () => import('./copy/content-picker-to-media-copy-translator.js'),
		fromPropertyEditorUi: forPropertyEditorUi,
		toClipboardEntryValueType: UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.MediaToContentPicker',
		name: 'Media To Content Picker Clipboard Paste Property Value Translator',
		weight: 900,
		api: () => import('./paste/media-to-content-picker-paste-translator.js'),
		fromClipboardEntryValueType: UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: forPropertyEditorUi,
	},
];
