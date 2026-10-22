import type { UmbElementVariantPickerData } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export const UMB_ELEMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.ElementSchedule';

export interface UmbElementScheduleSelectionModel {
	unique: string;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbElementScheduleModalData extends UmbElementVariantPickerData {
	activeVariants: Array<string>;
	prevalues: Array<UmbElementScheduleSelectionModel>;
	unique?: string;
	itemRepositoryAlias?: string;
	referenceRepositoryAlias?: string;
	/** Entities directly referenced by this entity's current draft that need attention before publishing. */
	entitiesNeedingAttention?: Array<UmbEntityModel>;
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
