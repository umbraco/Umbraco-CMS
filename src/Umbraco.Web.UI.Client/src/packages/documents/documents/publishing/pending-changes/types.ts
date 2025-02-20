import type { UmbDocumentDetailModel } from '../../types.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbDocumentPublishedPendingChangesManagerProcessArgs {
	persistedData: UmbDocumentDetailModel;
	publishedData: UmbDocumentDetailModel;
}

export interface UmbPublishedVariantWithPendingChanges {
	variantId: UmbVariantId;
}
