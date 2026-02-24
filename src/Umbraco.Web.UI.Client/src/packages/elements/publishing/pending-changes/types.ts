import type { UmbElementDetailModel } from '../../types.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbElementPublishedPendingChangesManagerProcessArgs {
	persistedData: UmbElementDetailModel;
	publishedData: UmbElementDetailModel;
}

export interface UmbPublishedElementVariantWithPendingChanges {
	variantId: UmbVariantId;
}
