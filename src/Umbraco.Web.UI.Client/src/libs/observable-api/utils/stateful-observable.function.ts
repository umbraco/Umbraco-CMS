import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

const statefulObservables = new WeakSet<Observable<unknown>>();

/**
 * Marks an Observable as stateful: it holds a current value and gives it to every new subscriber.
 *
 * Observables derived from stateful Observables, through `createObservablePart` or `mergeObservables`, release their
 * sources once they have no subscribers left, and subscribe again when a new subscriber arrives.
 * Observables of an Umbraco State (`UmbBasicState` or one of its subclasses, such as `UmbArrayState`), and
 * Observables derived only from States, are already stateful. Holding values in a State is the recommended approach,
 * and then marking is not needed.
 *
 * Only mark an Observable that gives every new subscriber its current value. Marking an Observable that only emits
 * values as they happen makes a subscriber that arrives after a release miss values until the next emission.
 * @param {Observable} observable - An Observable that gives every new subscriber its current value.
 * @returns {Observable} The same Observable.
 * @example
 * const items = umbMarkAsStatefulObservable(new BehaviorSubject<Array<MyItem>>([]));
 * const itemCount = createObservablePart(items, (items) => items.length);
 */
export function umbMarkAsStatefulObservable<T extends Observable<unknown>>(observable: T): T {
	statefulObservables.add(observable);
	return observable;
}

/**
 * Checks whether an Observable is marked as stateful, see {@link umbMarkAsStatefulObservable}.
 * @param {Observable} observable - The Observable to check.
 * @returns {boolean} Whether the Observable is marked as stateful.
 */
export function umbIsStatefulObservable(observable: Observable<unknown> | undefined): boolean {
	return !!observable && statefulObservables.has(observable);
}
