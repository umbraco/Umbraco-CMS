import { UMB_MEDIA_PICKER_CLIPBOARD_ENTRY_VALUE_TYPE } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'clipboardCopyPropertyValueTranslator',
		alias: 'Umb.ClipboardCopyPropertyValueTranslator.MediaPickerToMediaPicker',
		name: 'Media Picker Clipboard Copy Property Value Translator',
		api: () => import('./media-picker-to-media-picker-copy-translator.js'),
		fromPropertyEditorUi: 'Umb.PropertyEditorUi.MediaPicker',
		toClipboardEntryValueType: UMB_MEDIA_PICKER_CLIPBOARD_ENTRY_VALUE_TYPE,
	},
];

