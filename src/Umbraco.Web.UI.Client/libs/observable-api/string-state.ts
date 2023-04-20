import { UmbBasicState } from './basic-state';

/**
 * @export
 * @class UmbStringState
 * @extends {UmbBasicState<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class UmbStringState<T> extends UmbBasicState<T | string> {
	constructor(initialData: T | string) {
		super(initialData);
	}
}
