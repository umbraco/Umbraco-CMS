import type { UmbDocumentVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

export interface UmbDocumentScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentScheduleModalData extends UmbDocumentVariantPickerData {}

export interface UmbDocumentScheduleModalValue {
	selection: Array<UmbDocumentScheduleSelectionModel>;
}

export const UMB_DOCUMENT_SCHEDULE_MODAL = new UmbModalToken<
	UmbDocumentScheduleModalData,
	UmbDocumentScheduleModalValue
>(UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
