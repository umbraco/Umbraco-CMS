import { BehaviorSubject } from 'rxjs';

interface ClassStateData {
	equal(otherClass: ClassStateData): boolean;
}

/**
 * @export
 * @class ClassState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which can hold class instance which has a equal method to compare in coming instances for changes.
 */
export class ClassState<T extends ClassStateData | undefined | null> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(initialData);
	}

	next(newData: T): void {
		const oldValue = this.getValue();

		if (newData && oldValue?.equal(newData)) return;
		super.next(newData);
	}
}
