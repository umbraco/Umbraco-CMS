import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @export
 * @method observeMultiple
 * @param {Array<Observable<T>>} sources - an Array of Observables to use for this combined observation.
 * @description - combines multiple Observables into a single Observable that can be observed.
 */
export const observeMultiple = combineLatest;
