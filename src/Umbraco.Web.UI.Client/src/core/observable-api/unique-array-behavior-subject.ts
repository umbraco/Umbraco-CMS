import { appendToFrozenArray, UniqueBehaviorSubject } from "./unique-behavior-subject";

/**
 * @export
 * @class UniqueObjectBehaviorSubject
 * @extends {UniqueBehaviorSubject<T>}
 * @description - A RxJS UniqueObjectBehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The UniqueObjectBehaviorSubject provides methods to append data when the data is an Object.
 */

export class UniqueArrayBehaviorSubject<T> extends UniqueBehaviorSubject<T[]> {


	constructor(initialData: T[], private _uniqueCompare?: (existingEntry: T, newEntry: T) => boolean) {
		super(initialData);
	}

	/**
	 * @method append
	 * @param {Partial<T>} partialData - A object containing some of the data for this Subject.
	 * @description - Append some new data to this Object.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new UniqueArrayBehaviorSubject(data);
	 * mySubject.append({ key: 1, value: 'replaced-foo'});
	 */
	appendOne(entry: T) {
		this.next(appendToFrozenArray(this.getValue(), entry, this._uniqueCompare))
	}

	/**
	 * @method append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @description - Append some new data to this Object, if it compares to existing data it will replace it.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new UniqueArrayBehaviorSubject(data);
	 * mySubject.append([
	 * 	{ key: 1, value: 'replaced-foo'},
	 * 	{ key: 3, value: 'another-bla'}
	 * ]);
	 */
	append(entries: T[]) {
		// TODO: stop calling appendOne for each but make sure to handle this in one.
		entries.forEach(x => this.appendOne(x))
	}
}
