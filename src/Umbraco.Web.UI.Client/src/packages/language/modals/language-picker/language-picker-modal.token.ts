import type { UmbLanguageItemModel } from '../../repository/item/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLanguagePickerModalData {
	multiple?: boolean;
	/**
	 * @deprecated Deprecated since v17. Use `pickableFilter` to prevent languages from being picked instead. Scheduled for removal in Umbraco 21.
	 */
	filter?: (language: UmbLanguageItemModel) => boolean;
	/**
	 * Items for which this returns false are shown but cannot be picked.
	 */
	pickableFilter?: (language: UmbLanguageItemModel) => boolean;
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
