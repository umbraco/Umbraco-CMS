import type { UmbDocumentVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

export interface UmbDocumentScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbDocumentAncestorPublishCoverageModel {
	/** Whether every ancestor is published in at least one culture, which publishing requires. */
	isPathPublished: boolean;
	/** The cultures published in every ancestor, or `null` when every ancestor is published invariantly. */
	publishedCultures: Array<string> | null;
}

export interface UmbDocumentScheduleModalData extends UmbDocumentVariantPickerData {
	activeVariants: Array<string>;
	prevalues: Array<UmbDocumentScheduleSelectionModel>;
	/**
	 * How the document's ancestors cover a scheduled publish, used to warn when it will fail or
	 * won't be visible. `undefined` for a root document or when the lookup is unavailable.
	 */
	ancestorPublishCoverage?: UmbDocumentAncestorPublishCoverageModel;
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
