import { UniqueBehaviorSubject } from "./unique-behavior-subject";
import { appendToFrozenArray } from "./append-to-frozen-array.method";

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


	constructor(initialData: T[], private _getUnique?: (entry: T) => unknown) {
		super(initialData);
	}

	/**
	 * @method append
	 * @param {unknown} unique - The unique value to remove.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new UniqueArrayBehaviorSubject(data, (x) => x.key);
	 * mySubject.remove([1]);
	 */
	remove(uniques: unknown[]) {
		const unFrozenDataSet = [...this.getValue()];
		if (this._getUnique) {
			uniques.forEach( unique =>
				unFrozenDataSet.filter(x => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					return this._getUnique(x) !== unique;
				})
			);

			this.next(unFrozenDataSet);
		}
	}

	/**
	 * @method append
	 * @param {Partial<T>} partialData - A object containing some of the data for this Subject.
	 * @description - Append some new data to this Subject.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new UniqueArrayBehaviorSubject(data);
	 * mySubject.append({ key: 1, value: 'replaced-foo'});
	 */
	appendOne(entry: T) {
		this.next(appendToFrozenArray(this.getValue(), entry, this._getUnique))
	}

	/**
	 * @method append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @description - Append some new data to this Subject, if it compares to existing data it will replace it.
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
