import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS } from '../manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDocumentScheduleModalData extends UmbDocumentVariantPickerData {}

export interface UmbDocumentScheduleModalValue extends UmbDocumentVariantPickerValue {
	schedule?: ScheduleRequestModel;
}

export const UMB_DOCUMENT_SCHEDULE_MODAL = new UmbModalToken<
	UmbDocumentScheduleModalData,
	UmbDocumentScheduleModalValue
>(UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
