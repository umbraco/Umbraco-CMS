import { UmbFilterActiveValueApiBase } from '../base/filter-active-value-api.base.js';

/**
 * Default filter active value API. Provided by the `default` kind for `filterActiveValue` extensions.
 */
export class UmbDefaultFilterActiveValueApi<
	TValue = unknown,
	TResolvedValue = unknown,
> extends UmbFilterActiveValueApiBase<TValue, TResolvedValue> {}
