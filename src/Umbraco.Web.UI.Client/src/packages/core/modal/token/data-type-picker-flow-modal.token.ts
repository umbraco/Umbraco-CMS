import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowModalData {
	submitLabel?: string;
}

export type UmbDataTypePickerFlowModalValue = {
	selection: Array<string>;
};

export const UMB_DATA_TYPE_PICKER_FLOW_MODAL = new UmbModalToken<
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalValue
>('Umb.Modal.DataTypePickerFlow', {
	config: {
		type: 'sidebar',
		size: 'small',
	},
});
