import { LanguageModel } from '@umbraco-cms/backend-api';
import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbLanguagePickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
	filter?: (language: LanguageModel) => boolean;
}

export interface UmbLanguagePickerModalResult {
	selection: Array<string>;
}

export const UMB_LANGUAGE_PICKER_MODAL_TOKEN = new UmbModalToken<
	UmbLanguagePickerModalData,
	UmbLanguagePickerModalResult
>('Umb.Modal.LanguagePicker', {
	type: 'sidebar',
	size: 'small',
});
