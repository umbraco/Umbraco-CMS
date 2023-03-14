import { BasicState } from './basic-state';

/**
 * @export
 * @class BooleanState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class BooleanState<T> extends BasicState<T | boolean> {
	constructor(initialData: T | boolean) {
		super(initialData);
	}
}
