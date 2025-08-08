import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @function observationAsPromise
 * @param {Observable<unknown>} observable - an Array of Observables to use for this combined observation.
 * @param {Promise<condition>} condition - an method which should return true or false, if rejected or returning undefined the observation will result in a rejected Promise.
 * @description - TODO:...
 * @returns {Promise<unknown>} - Returns a Promise which resolves when the condition returns true or rejects when the condition returns undefined or is rejecting it self.
 * @example
 *
 * TODO: ...
 */
export function observationAsPromise<T>(
	observable: Observable<T>,
	condition: (value: T) => Promise<boolean | undefined>,
): Promise<T> {
	// Notice, we do not want to store and reuse the Promise, cause this promise guarantees that the value is not undefined when resolved. and reusing the promise would not ensure that.
	return new Promise<T>((resolve, reject) => {
		let initialCallback = true;
		let wantedToClose = false;
		const subscription = observable.subscribe(async (value) => {
			const shouldClose = await condition(value).catch(() => {
				if (initialCallback) {
					wantedToClose = true;
				} else {
					subscription.unsubscribe();
				}
				reject(value);
			});
			if (shouldClose === true) {
				if (initialCallback) {
					wantedToClose = true;
				} else {
					subscription.unsubscribe();
				}
				resolve(value);
			}
		});
		initialCallback = false;
		if (wantedToClose) {
			subscription.unsubscribe();
		}
	});
}
