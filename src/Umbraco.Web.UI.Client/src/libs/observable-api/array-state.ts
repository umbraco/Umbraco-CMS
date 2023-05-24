import { UmbDeepState } from './deep-state.js';
import { partialUpdateFrozenArray } from './partial-update-frozen-array.function.js';
import { pushToUniqueArray } from './push-to-unique-array.function.js';

/**
 * @export
 * @class UmbArrayState
 * @extends {UmbDeepState<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The ArrayState provides methods to append data when the data is an Object.
 */
export class UmbArrayState<T> extends UmbDeepState<T[]> {
	#getUnique?: (entry: T) => unknown;
	#sortMethod?: (a: T, b: T) => number;

	constructor(initialData: T[], getUniqueMethod?: (entry: T) => unknown) {
		super(initialData);
		this.#getUnique = getUniqueMethod;
	}

	/**
	 * @method sortBy
	 * @param {(a: T, b: T) => number} sortMethod - A method to be used for sorting everytime data is set.
	 * @description - A sort method to this Subject.
	 * @example <caption>Example add sort method</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.key);
	 * myState.sortBy((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
	 */
	sortBy(sortMethod?: (a: T, b: T) => number) {
		this.#sortMethod = sortMethod;
		return this;
	}

	next(value: T[]) {
		if (this.#sortMethod) {
			super.next(value.sort(this.#sortMethod));
		} else {
			super.next(value);
		}
	}

	/**
	 * @method remove
	 * @param {unknown[]} uniques - The unique values to remove.
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with id '1' and '2'</caption>
	 * const data = [
	 * 	{ id: 1, value: 'foo'},
	 * 	{ id: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.id);
	 * myState.remove([1, 2]);
	 */
	remove(uniques: unknown[]) {
		let next = this.getValue();
		if (this.#getUnique) {
			uniques.forEach((unique) => {
				next = next.filter((x) => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					return this.#getUnique(x) !== unique;
				});
			});

			this.next(next);
		}
		return this;
	}

	/**
	 * @method removeOne
	 * @param {unknown} unique - The unique value to remove.
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with id '1'</caption>
	 * const data = [
	 * 	{ id: 1, value: 'foo'},
	 * 	{ id: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.id);
	 * myState.removeOne(1);
	 */
	removeOne(unique: unknown) {
		let next = this.getValue();
		if (this.#getUnique) {
			next = next.filter((x) => {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				return this.#getUnique(x) !== unique;
			});

			this.next(next);
		}
		return this;
	}

	/**
	 * @method filter
	 * @param {unknown} filterMethod - The unique value to remove.
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Remove some new data of this Subject.
	 * @example <caption>Example remove entry with key '1'</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'},
	 * 	{ key: 3, value: 'poo'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.key);
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
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Append some new data to this Subject.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data);
	 * myState.append({ key: 1, value: 'replaced-foo'});
	 */
	appendOne(entry: T) {
		const next = [...this.getValue()];
		if (this.#getUnique) {
			pushToUniqueArray(next, entry, this.#getUnique);
		} else {
			next.push(entry);
		}
		this.next(next);
		return this;
	}

	/**
	 * @method append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Append some new data to this Subject, if it compares to existing data it will replace it.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data);
	 * myState.append([
	 * 	{ key: 1, value: 'replaced-foo'},
	 * 	{ key: 3, value: 'another-bla'}
	 * ]);
	 */
	append(entries: T[]) {
		if (this.#getUnique) {
			const next = [...this.getValue()];
			entries.forEach((entry) => {
				pushToUniqueArray(next, entry, this.#getUnique!);
			});
			this.next(next);
		} else {
			this.next([...this.getValue(), ...entries]);
		}
		return this;
	}

	/**
	 * @method updateOne
	 * @param {unknown} unique - Unique value to find entry to update.
	 * @param {Partial<T>} entry - new data to be added in this Subject.
	 * @return {UmbArrayState<T>} Reference to it self.
	 * @description - Update a item with some new data, requires the ArrayState to be constructed with a getUnique method.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.key);
	 * myState.updateOne(2, {value: 'updated-bar'});
	 */
	updateOne(unique: unknown, entry: Partial<T>) {
		if (!this.#getUnique) {
			throw new Error("Can't partial update an ArrayState without a getUnique method provided when constructed.");
		}
		this.next(partialUpdateFrozenArray(this.getValue(), entry, (x) => unique === this.#getUnique!(x)));
		return this;
	}
}
