import type { UmbDocumentDetailModel, UmbDocumentVariantModel } from '../../types.js';
import { UmbPublishedPendingChangesManagerBase } from '@umbraco-cms/backoffice/content';

/**
 * Manages the pending changes for a published document.
 * @exports
 * @class UmbDocumentPublishedPendingChangesManager
 * @augments {UmbPublishedPendingChangesManagerBase}
 */
export class UmbDocumentPublishedPendingChangesManager extends UmbPublishedPendingChangesManagerBase<
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
