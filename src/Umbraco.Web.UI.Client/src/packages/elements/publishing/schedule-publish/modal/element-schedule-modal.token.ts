import type { UmbElementVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_ELEMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.ElementSchedule';

export interface UmbElementScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbElementScheduleModalData extends UmbElementVariantPickerData {
	activeVariants: Array<string>;
	prevalues: Array<UmbElementScheduleSelectionModel>;
}

export interface UmbElementScheduleModalValue {
	selection: Array<UmbElementScheduleSelectionModel>;
}

export const UMB_ELEMENT_SCHEDULE_MODAL = new UmbModalToken<UmbElementScheduleModalData, UmbElementScheduleModalValue>(
	UMB_ELEMENT_SCHEDULE_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
