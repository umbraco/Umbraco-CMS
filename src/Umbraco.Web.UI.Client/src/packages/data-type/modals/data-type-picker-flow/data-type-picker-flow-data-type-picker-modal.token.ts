import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowDataTypePickerModalData {
	propertyEditorUiAlias: string;
}

export type UmbDataTypePickerFlowDataTypePickerModalValue =
	| {
			dataTypeId?: string;
			createNewWithPropertyEditorUiAlias?: string;
	  }
	| undefined;

export const UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalValue
>('Umb.Modal.DataTypePickerFlowDataTypePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
