import { UmbBasicState } from './basic-state';

/**
 * @export
 * @class UmbBooleanState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class UmbBooleanState<T> extends UmbBasicState<T | boolean> {
	constructor(initialData: T | boolean) {
		super(initialData);
	}
}
