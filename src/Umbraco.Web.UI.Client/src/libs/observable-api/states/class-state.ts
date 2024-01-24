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

	/**
	 * @method setValue
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	setValue(data: T): void {
		const oldValue = this._subject.getValue();

		if (data && oldValue?.equal(data)) return;
		this._subject.next(data);
	}
}
