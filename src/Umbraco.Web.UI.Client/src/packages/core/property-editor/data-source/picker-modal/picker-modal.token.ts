import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UmbModalToken, type UmbPickerModalData, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalData
	extends UmbPickerModalData<UmbPropertyEditorDataSourceItemModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalValue extends UmbPickerModalValue {}

export const UMB_PROPERTY_EDITOR_DATA_SOURCE_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorDataSourcePickerModalData,
	UmbPropertyEditorDataSourcePickerModalValue
>('Umb.Modal.PropertyEditorDataSourcePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
