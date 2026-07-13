import type { UmbDocumentDetailModel, UmbDocumentVariantModel } from '../../types.js';
import { UmbContentPublishedPendingChangesManagerBase } from '@umbraco-cms/backoffice/content';

/**
 * Manages the pending changes for a published document.
 * @exports
 * @class UmbDocumentPublishedPendingChangesManager
 * @augments {UmbContentPublishedPendingChangesManagerBase}
 */
export class UmbDocumentPublishedPendingChangesManager extends UmbContentPublishedPendingChangesManagerBase<
	UmbDocumentDetailModel,
	UmbDocumentVariantModel
> {
	protected override cleanDataBeforeComparison(
		mergedData: UmbDocumentDetailModel,
		publishedData: UmbDocumentDetailModel,
	): void {
		// remove template from the comparison (doesn't affect publishable changes, and the published version is coming through as null)
		mergedData.template = null;
		publishedData.template = null;
	}
}
