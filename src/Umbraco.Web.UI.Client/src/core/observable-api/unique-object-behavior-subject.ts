import { UniqueBehaviorSubject } from "./unique-behavior-subject";

/**
 * @export
 * @class UniqueObjectBehaviorSubject
 * @extends {UniqueBehaviorSubject<T>}
 * @description - A RxJS UniqueObjectBehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The UniqueObjectBehaviorSubject provides methods to append data when the data is an Object.
 */
export class UniqueObjectBehaviorSubject<T> extends UniqueBehaviorSubject<T> {

	/**
	 * @method append
	 * @param {Partial<T>} partialData - A object containing some of the data for this Subject.
	 * @description - Append some new data to this Object.
	 * @example <caption>Example append some data.</caption>
	 * const data = {key: 'myKey', value: 'myInitialValue'};
	 * const mySubject = new UniqueObjectBehaviorSubject(data)
	 * mySubject.append({value: 'myNewValue'})
	 */
	update(partialData: Partial<T>) {
		this.next({ ...this.getValue(), ...partialData });
	}
}
