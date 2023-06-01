import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowDataTypePickerModalData {
	selection?: Array<string>;
	propertyEditorUiAlias: string;
}

export type UmbDataTypePickerFlowDataTypePickerModalResult = undefined;

export const UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalResult
>('Umb.Modal.DataTypePickerFlowDataTypePicker', {
	type: 'sidebar',
	size: 'small',
});
