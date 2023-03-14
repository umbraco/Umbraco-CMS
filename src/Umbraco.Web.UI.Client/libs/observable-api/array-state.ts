import { DeepState } from './deep-state';
import { pushToUniqueArray } from './push-to-unique-array.function';

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
	 * @method remove
	 * @param {unknown[]} uniques - The unique values to remove.
	 * @return {ArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1' and '2'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new ArrayState(data, (x) => x.key);
	 * myState.remove([1, 2]);
	 */
	remove(uniques: unknown[]) {
		let next = this.getValue();
		if (this._getUnique) {
			uniques.forEach((unique) => {
				next = next.filter((x) => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					return this._getUnique(x) !== unique;
				});
			});

			this.next(next);
		}
		return this;
	}

	/**
	 * @method removeOne
	 * @param {unknown} unique - The unique value to remove.
	 * @return {ArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new ArrayState(data, (x) => x.key);
	 * myState.removeOne(1);
	 */
	removeOne(unique: unknown) {
		let next = this.getValue();
		if (this._getUnique) {
			next = next.filter((x) => {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				return this._getUnique(x) !== unique;
			});

			this.next(next);
		}
		return this;
	}

	/**
	 * @method filter
	 * @param {unknown} filterMethod - The unique value to remove.
	 * @return {ArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'},
	 * 	{ key: 3, value: 'poo'}
	 * ];
	 * const myState = new ArrayState(data, (x) => x.key);
	 * myState.filter((entry) => entry.key !== 1);
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
	 * @method appendOne
	 * @param {T} entry - new data to be added in this Subject.
	 * @return {ArrayState<T>} Reference to it self.
	 * @description - Append some new data to this Subject.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new ArrayState(data);
	 * myState.append({ key: 1, value: 'replaced-foo'});
	 */
	appendOne(entry: T) {
		const next = [...this.getValue()];
		if (this._getUnique) {
			pushToUniqueArray(next, entry, this._getUnique);
		} else {
			next.push(entry);
		}
		this.next(next);
		return this;
	}

	/**
	 * @method append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @return {ArrayState<T>} Reference to it self.
	 * @description - Append some new data to this Subject, if it compares to existing data it will replace it.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new ArrayState(data);
	 * myState.append([
	 * 	{ key: 1, value: 'replaced-foo'},
	 * 	{ key: 3, value: 'another-bla'}
	 * ]);
	 */
	append(entries: T[]) {
		if (this._getUnique) {
			const next = [...this.getValue()];
			entries.forEach((entry) => {
				pushToUniqueArray(next, entry, this._getUnique!);
			});
			this.next(next);
		} else {
			this.next([...this.getValue(), ...entries]);
		}
		return this;
	}
}
