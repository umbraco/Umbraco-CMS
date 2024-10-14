import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerFlowModalData {
	/** @deprecated This property will be removed in Umbraco 15. */
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
