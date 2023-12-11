import { UmbBasicState } from './basic-state.js';

export interface UmbClassStateData {
	equal(otherClass: this | undefined): boolean;
}

/**
 * @export
 * @class UmbClassState
 * @extends {UmbBasicState<T>}
 * @description - This state can hold class instance which has a equal method to compare in coming instances for changes.
 */
export class UmbClassState<T extends UmbClassStateData | undefined> extends UmbBasicState<T> {
	constructor(initialData: T) {
		super(initialData);
	}

	next(newData: T): void {
		const oldValue = this._subject.getValue();

		if (newData && oldValue?.equal(newData)) return;
		this._subject.next(newData);
	}
}
