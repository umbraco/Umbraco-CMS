import type { CodeSnippetType } from '../../types.js';
import type { UmbPartialViewPickerModalValue, UmbTemplatingPageFieldBuilderModalValue } from '../index.js';
import { type UmbDictionaryItemPickerModalValue, UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplatingItemPickerModalData {
	hidePartialViews?: boolean;
}

export type UmbTemplatingItemPickerModalValue = {
	value: string;
	type: CodeSnippetType;
};

export const UMB_TEMPLATING_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbTemplatingItemPickerModalData,
	UmbTemplatingItemPickerModalValue
>('Umb.Modal.TemplatingItemPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
