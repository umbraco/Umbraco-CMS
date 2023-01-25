import { DeepState } from "./deep-state";

/**
 * @export
 * @class ObjectState
 * @extends {DeepState<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The ObjectState provides methods to append data when the data is an Object.
 */
export class ObjectState<T> extends DeepState<T> {

	/**
	 * @method update
	 * @param {Partial<T>} partialData - A object containing some of the data to update in this Subject.
	 * @description - Append some new data to this Object.
	 * @return {ObjectState<T>} Reference to it self.
	 * @example <caption>Example append some data.</caption>
	 * const data = {key: 'myKey', value: 'myInitialValue'};
	 * const myState = new ObjectState(data);
	 * myState.update({value: 'myNewValue'});
	 */
	update(partialData: Partial<T>) {
		this.next({ ...this.getValue(), ...partialData });
		return this;
	}
}
