import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLanguagePickerModalData {
	multiple?: boolean;
	filter?: (language: LanguageItemResponseModel) => boolean;
}

export interface UmbLanguagePickerModalValue {
	selection: Array<string | null>;
}

export const UMB_LANGUAGE_PICKER_MODAL = new UmbModalToken<UmbLanguagePickerModalData, UmbLanguagePickerModalValue>(
	'Umb.Modal.LanguagePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
