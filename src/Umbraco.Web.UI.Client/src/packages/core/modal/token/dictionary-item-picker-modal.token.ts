import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDictionaryItemPickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbDictionaryItemPickerModalResult = UmbPickerModalResult;

export const UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS = 'Umb.Modal.DictionaryItemPicker';

export const UMB_DICTIONARY_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryItemPickerModalData,
	UmbDictionaryItemPickerModalResult
>(
	UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Dictionary',
	}
);
