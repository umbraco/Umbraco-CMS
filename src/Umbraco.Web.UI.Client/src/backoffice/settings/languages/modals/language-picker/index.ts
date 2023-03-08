import { LanguageModel } from '@umbraco-cms/backend-api';
import { UmbModalToken } from 'libs/modal';

export interface UmbLanguagePickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
	filter?: (language: LanguageModel) => boolean;
}

export interface UmbLanguagePickerModalResponse {
	selection: Array<string>;
}

export const UMB_LANGUAGE_PICKER_MODAL_TOKEN = new UmbModalToken<UmbLanguagePickerModalData>(
	'Umb.Modal.LanguagePicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
