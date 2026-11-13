import { UMB_RICH_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE } from '../../../../clipboard/constants.js';

const forPropertyEditorUi = 'Umb.PropertyEditorUi.MediaPicker';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.MediaPickerToRichMedia',
		name: 'Media Picker To Rich Media Clipboard Copy Property Value Translator',
		api: () => import('./copy/media-picker-to-rich-media-copy-translator.js'),
		fromPropertyEditorUi: forPropertyEditorUi,
		toClipboardEntryValueType: UMB_RICH_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.RichMediaToMediaPicker',
		name: 'Rich Media To Media Picker Clipboard Paste Property Value Translator',
		weight: 1000,
		api: () => import('./paste/rich-media-to-media-picker-paste-translator.js'),
		fromClipboardEntryValueType: UMB_RICH_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: forPropertyEditorUi,
	},
];
