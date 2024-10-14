import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @function observeMultiple
 * @param {Array<Observable<T>>} sources - an Array of Observables to use for this combined observation.
 * @description - combines multiple Observables into a single Observable that can be observed.
 * @returns {Observable<Array<T>>} - Returns a new Observable that combines the Observables into a single Observable with the values of the given Observables in an Array with the same order as the Array of Observables.
 * @example
 *
 * this.observe(observeMultiple([observable1, observable2]), ([value1, value2]) => {
 * 	console.log(value1, value2);
 * });
 */
export const observeMultiple = combineLatest;
