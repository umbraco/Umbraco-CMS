import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowDataTypePickerModalData {
	propertyEditorUiAlias: string;
}

export type UmbDataTypePickerFlowDataTypePickerModalResult = {
	dataTypeId?: string;
	createNewWithPropertyEditorUiAlias?: string;
};

export const UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalResult
>('Umb.Modal.DataTypePickerFlowDataTypePicker', {
	type: 'sidebar',
	size: 'medium',
});
