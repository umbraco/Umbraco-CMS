import { UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE } from '../../../../clipboard/constants.js';

const forPropertyEditorUi = 'Umb.PropertyEditorUi.MediaPicker';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.MediaPickerToMedia',
		name: 'Media Picker To Media Clipboard Copy Property Value Translator',
		api: () => import('./copy/media-picker-to-media-copy-translator.js'),
		fromPropertyEditorUi: forPropertyEditorUi,
		toClipboardEntryValueType: UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.MediaToMediaPicker',
		name: 'Media To Media Picker Clipboard Paste Property Value Translator',
		weight: 900,
		api: () => import('./paste/media-to-media-picker-paste-translator.js'),
		fromClipboardEntryValueType: UMB_MEDIA_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: forPropertyEditorUi,
	},
];
