import { UmbModalToken } from './modal-token.js';

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
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
