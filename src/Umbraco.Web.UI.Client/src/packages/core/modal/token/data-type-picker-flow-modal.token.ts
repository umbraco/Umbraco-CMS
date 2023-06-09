import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowModalData {
	selection?: Array<string>;
	submitLabel?: string;
}

export type UmbDataTypePickerFlowModalResult = {
	selection: Array<string>;
};

export const UMB_DATA_TYPE_PICKER_FLOW_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalResult
>('Umb.Modal.DataTypePickerFlow', {
	type: 'sidebar',
	size: 'medium',
});
