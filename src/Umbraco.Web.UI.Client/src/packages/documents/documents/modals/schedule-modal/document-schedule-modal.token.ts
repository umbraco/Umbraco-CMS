import type { UmbDocumentVariantPickerData } from '../types.js';
import { UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS } from './manifest.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

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
