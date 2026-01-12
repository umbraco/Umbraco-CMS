import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbElementVariantPublishModel {
	variantId: UmbVariantId;
	schedule?: ScheduleRequestModel | null;
}

export type * from './publish/types.js';
export type * from './unpublish/types.js';
export type * from './schedule-publish/modal/types.js';
