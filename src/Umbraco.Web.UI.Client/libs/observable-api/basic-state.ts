import { BehaviorSubject } from "rxjs";

/**
 * @export
 * @class BasicState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject this Subject ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class BasicState<T extends string | number | undefined | null> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(initialData);
	}

	next(newData: T): void {
		if(newData !== this.getValue()) {
			super.next(newData);
		}
	}
}
