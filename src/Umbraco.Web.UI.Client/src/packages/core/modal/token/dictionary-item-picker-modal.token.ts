import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDictionaryItemPickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbDictionaryItemPickerModalValue = UmbPickerModalValue;

export const UMB_DICTIONARY_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryItemPickerModalData,
	UmbDictionaryItemPickerModalValue
>('Umb.Modal.TreePicker', {
	config: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.Dictionary',
	},
});
