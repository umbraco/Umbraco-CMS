import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @function observationAsPromise
 * @param {Observable<unknown>} observable - an Array of Observables to use for this combined observation.
 * @param {Promise<condition>} condition - a method which should return true or false, if rejected or returning undefined the observation will result in a rejected Promise.
 * @description - Observes an Observable and returns a Promise that resolves when the condition returns true. If the condition returns undefined or rejects, the Promise will reject with the current value.
 * @returns {Promise<unknown>} - Returns a Promise which resolves when the condition returns true or rejects when the condition returns undefined or is rejecting it self.
 */
export function observationAsPromise<T>(
	observable: Observable<T>,
	condition: (value: T) => Promise<boolean | undefined>,
): Promise<T> {
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
