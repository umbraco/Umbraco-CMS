import type { UmbElementDetailModel, UmbElementVariantModel } from '../../types.js';
import { UmbContentPublishedPendingChangesManagerBase } from '@umbraco-cms/backoffice/content';

/**
 * Manages the pending changes for a published element.
 * @exports
 * @class UmbElementPublishedPendingChangesManager
 * @augments {UmbContentPublishedPendingChangesManagerBase}
 */
export class UmbElementPublishedPendingChangesManager extends UmbContentPublishedPendingChangesManagerBase<
	UmbElementDetailModel,
	UmbElementVariantModel
> {}
