import { BasicState } from "./basic-state";

/**
 * @export
 * @class StringState
 * @extends {BasicState<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class StringState<T> extends BasicState<T | string> {
	constructor(initialData: T | string) {
		super(initialData);
	}
}
