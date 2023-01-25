import { DeepState } from "./deep-state";
import { appendToFrozenArray } from "./append-to-frozen-array.method";

/**
 * @export
 * @class ArrayState
 * @extends {DeepState<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The ArrayState provides methods to append data when the data is an Object.
 */

export class ArrayState<T> extends DeepState<T[]> {


	constructor(initialData: T[], private _getUnique?: (entry: T) => unknown) {
		super(initialData);
	}

	/**
	 * @method append
	 * @param {unknown} unique - The unique value to remove.
	 * @returns ArrayState<T>
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new ArrayState(data, (x) => x.key);
	 * mySubject.remove([1]);
	 */
	remove(uniques: unknown[]) {
		let next = this.getValue();
		if (this._getUnique) {
			uniques.forEach( unique => {
					next = next.filter(x => {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						return this._getUnique(x) !== unique;
					})
				}
			);

			this.next(next);
		}
		return this;
	}

	/**
	 * @method filter
	 * @param {unknown} filterMethod - The unique value to remove.
	 * @returns ArrayState<T>
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'},
	 * 	{ key: 3, value: 'poo'}
	 * ];
	 * const mySubject = new ArrayState(data, (x) => x.key);
	 * mySubject.filter((entry) => entry.key !== 1);
	 *
	 * Result:
	 *  [
	 * 		{ key: 2, value: 'bar'},
	 * 		{ key: 3, value: 'poo'}
	 * ]
	 *
	 */
	filter(predicate: (value: T, index: number, array: T[]) => boolean) {
		this.next(this.getValue().filter(predicate));
		return this;
	}

	/**
	 * @method append
	 * @param {Partial<T>} partialData - A object containing some of the data for this Subject.
	 * @returns ArrayState<T>
	 * @description - Append some new data to this Subject.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new ArrayState(data);
	 * mySubject.append({ key: 1, value: 'replaced-foo'});
	 */
	appendOne(entry: T) {
		this.next(appendToFrozenArray(this.getValue(), entry, this._getUnique))
		return this;
	}

	/**
	 * @method append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @returns ArrayState<T>
	 * @description - Append some new data to this Subject, if it compares to existing data it will replace it.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const mySubject = new ArrayState(data);
	 * mySubject.append([
	 * 	{ key: 1, value: 'replaced-foo'},
	 * 	{ key: 3, value: 'another-bla'}
	 * ]);
	 */
	append(entries: T[]) {
		// TODO: stop calling appendOne for each but make sure to handle this in one.
		entries.forEach(x => this.appendOne(x))

		/*
		const unFrozenDataSet = [...this.getValue()];

		this.next(unFrozenDataSet);
		*/
		return this;
	}
}
