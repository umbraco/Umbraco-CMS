import type { UmbDocumentVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

export interface UmbDocumentScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbDocumentScheduleModalData extends UmbDocumentVariantPickerData {
	activeVariants: Array<string>;
	/**
	 * The variant the editor is working on. Its dates are the source when scheduling every selected variant at once;
	 * when omitted, that option is unavailable.
	 */
	currentVariant?: string;
	prevalues: Array<UmbDocumentScheduleSelectionModel>;
}

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
