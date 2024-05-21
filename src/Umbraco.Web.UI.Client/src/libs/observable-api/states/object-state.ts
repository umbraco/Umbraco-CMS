import { UmbDeepState } from './deep-state.js';

/**
 * @export
 * @class UmbObjectState
 * @extends {UmbDeepState<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The UmbObjectState provides methods to append data when the data is an Object.
 */
export class UmbObjectState<T> extends UmbDeepState<T> {
	#partialUpdateData?: Partial<T>;
	#partialUpdateDebounce?: boolean;

	/**
	 * @method update
	 * @param {Partial<T>} partialData - A object containing some of the data to update in this Subject.
	 * @description - Append some new data to this Object.
	 * @return {UmbObjectState<T>} Reference to it self.
	 * @example <caption>Example append some data.</caption>
	 * const data = {key: 'myKey', value: 'myInitialValue'};
	 * const myState = new UmbObjectState(data);
	 * myState.update({value: 'myNewValue'});
	 */

	update(partialData: Partial<T>) {
		this.setValue({ ...this._subject.getValue(), ...partialData });
		return this;
	}
	/*
	update(partialData: Partial<T>) {
		this.#partialUpdateData = { ...this.#partialUpdateData, ...partialData };
		this.#performUpdate();
		return this;
	}

	async #performUpdate() {
		if (this.#partialUpdateDebounce) return;
		this.#partialUpdateDebounce = true;
		await Promise.resolve();
		if (this.#partialUpdateData) {
			this.setValue({ ...this._subject.getValue(), ...this.#partialUpdateData });
			this.#partialUpdateData = undefined;
		}
		this.#partialUpdateDebounce = false;
	}

	//getValue? — should this also include the partial update? but be aware that getValue is used for setValue comparison....!! [NL]

	setValue(data: T): void {
		if (this.#partialUpdateData) {
			console.error('SetValue was called while in debouncing mode.');
			super.setValue({ ...data, ...this.#partialUpdateData });
			//this.#partialUpdateData = undefined; // maybe not, cause keeping this enables that to be merged in despite a another change coming from above.
		} else {
			super.setValue(data);
		}
	}
	*/
}
