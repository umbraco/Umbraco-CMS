import { UMB_MEDIA_PICKER_CLIPBOARD_ENTRY_VALUE_TYPE } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardPastePropertyValueTranslator',
		alias: 'Umb.ClipboardPastePropertyValueTranslator.MediaPickerFromMediaPicker',
		name: 'Media Picker Clipboard Paste Property Value Translator',
		api: () => import('./media-picker-from-media-picker-paste-translator.js'),
		fromClipboardEntryValueType: UMB_MEDIA_PICKER_CLIPBOARD_ENTRY_VALUE_TYPE,
		toPropertyEditorUi: 'Umb.PropertyEditorUi.MediaPicker',
	},
];

