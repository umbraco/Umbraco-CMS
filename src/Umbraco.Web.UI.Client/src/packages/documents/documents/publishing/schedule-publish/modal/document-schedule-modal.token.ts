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
	prevalues: Array<UmbDocumentScheduleSelectionModel>;
	/**
	 * Cultures published across the entire ancestor chain — i.e. the intersection of
	 * each ancestor's published cultures. Used to warn when a scheduled publish won't
	 * take effect because an ancestor isn't published in that culture.
	 *
	 * - `undefined` — root document or lookup unavailable; no warnings rendered.
	 * - `[null]` — every ancestor is published in the invariant variant (covers all child cultures).
	 * - `[]` — no culture is published in every ancestor; every variant is warned.
	 */
	ancestorPublishedCultures?: Array<string | null>;
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
