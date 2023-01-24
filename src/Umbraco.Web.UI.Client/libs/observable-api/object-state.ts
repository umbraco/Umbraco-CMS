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
	 * @method append
	 * @param {Partial<T>} partialData - A object containing some of the data for this Subject.
	 * @description - Append some new data to this Object.
	 * @example <caption>Example append some data.</caption>
	 * const data = {key: 'myKey', value: 'myInitialValue'};
	 * const mySubject = new ObjectState(data)
	 * mySubject.append({value: 'myNewValue'})
	 */
	update(partialData: Partial<T>) {
		this.next({ ...this.getValue(), ...partialData });
	}
}
