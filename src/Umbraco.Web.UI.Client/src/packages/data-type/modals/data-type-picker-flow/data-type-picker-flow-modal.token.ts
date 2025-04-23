import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbDataTypePickerFlowModalData = object;

export type UmbDataTypePickerFlowModalValue = {
	selection: Array<string>;
};

export const UMB_DATA_TYPE_PICKER_FLOW_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalValue
>('Umb.Modal.DataTypePickerFlow', {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
