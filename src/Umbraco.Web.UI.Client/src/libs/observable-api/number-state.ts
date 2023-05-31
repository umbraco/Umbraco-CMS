import { UmbBasicState } from './basic-state.js';

/**
 * @export
 * @class UmbNumberState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class UmbNumberState<T> extends UmbBasicState<T | number> {
	constructor(initialData: T | number) {
		super(initialData);
	}
}
