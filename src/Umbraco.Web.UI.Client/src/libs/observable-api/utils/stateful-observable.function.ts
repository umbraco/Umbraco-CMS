import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

const statefulObservables = new WeakSet<Observable<unknown>>();

/**
 * Marks an Observable as stateful: it holds a current value and gives it to every new subscriber, like the Observables of a State and those derived only from States.
 * Observables derived from stateful observables release their sources once they have no subscribers left.
 * @param {Observable} observable - An Observable that gives every new subscriber its current value.
 * @returns {Observable} The same Observable.
 * @internal
 */
export function umbMarkAsStatefulObservable<T extends Observable<unknown>>(observable: T): T {
	statefulObservables.add(observable);
	return observable;
}

/**
 * @param {Observable} observable - The Observable to check.
 * @returns {boolean} Whether the Observable is marked as a stateful observable.
 * @internal
 */
export function umbIsStatefulObservable(observable: Observable<unknown> | undefined): boolean {
	return !!observable && statefulObservables.has(observable);
}
