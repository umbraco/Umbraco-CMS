import type { UmbDocumentVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

export interface UmbDocumentScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbDocumentScheduleModalData extends UmbDocumentVariantPickerData {
	activeVariants: Array<string>;
	prevalues: Array<UmbDocumentScheduleSelectionModel>;
	unique?: string;
	itemRepositoryAlias?: string;
	referenceRepositoryAlias?: string;
	/** Entities directly referenced by this entity's current draft that need attention before publishing. */
	entitiesNeedingAttention?: Array<UmbEntityModel>;
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
