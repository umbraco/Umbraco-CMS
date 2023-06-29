import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

interface UmbClassStateData {
	equal(otherClass: UmbClassStateData | undefined): boolean;
}

/**
 * @export
 * @class UmbClassState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which can hold class instance which has a equal method to compare in coming instances for changes.
 */
export class UmbClassState<T extends UmbClassStateData | undefined> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(initialData);
	}

	next(newData: T): void {
		const oldValue = this.getValue();

		if (newData && oldValue?.equal(newData)) return;
		super.next(newData);
	}
}
