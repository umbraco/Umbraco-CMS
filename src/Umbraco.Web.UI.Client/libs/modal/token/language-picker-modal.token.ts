import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLanguagePickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
	filter?: (language: LanguageResponseModel) => boolean;
}

export interface UmbLanguagePickerModalResult {
	selection: Array<string>;
}

export const UMB_LANGUAGE_PICKER_MODAL = new UmbModalToken<UmbLanguagePickerModalData, UmbLanguagePickerModalResult>(
	'Umb.Modal.LanguagePicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
