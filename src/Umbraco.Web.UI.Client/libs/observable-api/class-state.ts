import { BehaviorSubject } from 'rxjs';

interface ClassStateData {
	equal(otherClass: ClassStateData): boolean;
}

/**
 * @export
 * @class DeepState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
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
