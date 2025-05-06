import { partialUpdateFrozenArray } from '../utils/partial-update-frozen-array.function.js';
import { pushAtToUniqueArray } from '../utils/push-at-to-unique-array.function.js';
import { pushToUniqueArray } from '../utils/push-to-unique-array.function.js';
import { UmbDeepState } from './deep-state.js';

/**
 * @class UmbArrayState
 * @augments {UmbDeepState<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the object-data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 *
 * The ArrayState provides methods to append data when the data is an Object.
 */
export class UmbArrayState<T, U = unknown> extends UmbDeepState<T[]> {
	readonly getUniqueMethod: (entry: T) => U;
	#sortMethod?: (a: T, b: T) => number;

	constructor(initialData: T[], getUniqueOfEntryMethod: (entry: T) => U) {
		super(initialData);
		this.getUniqueMethod = getUniqueOfEntryMethod;
	}

	/**
	 * @function sortBy
	 * @param {(a: T, b: T) => number} sortMethod - A method to be used for sorting every time data is set.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		const value = this.getValue();
		if (value) {
			super.setValue([...value].sort(this.#sortMethod));
		}
		return this;
	}

	/**
	 * @function setValue
	 * @param value
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if sortBy has been defined for this state the data will be sorted before set. If data is different than current this will trigger observations to update.
	 * @example <caption>Example change the data of a state</caption>
	 * const myState = new UmbArrayState('Good morning');
	 * // myState.value is equal 'Good morning'.
	 * myState.setValue('Goodnight')
	 * // myState.value is equal 'Goodnight'.
	 */
	override setValue(value: T[]) {
		if (value && this.#sortMethod) {
			super.setValue([...value].sort(this.#sortMethod));
		} else {
			super.setValue(value);
		}
	}

	/**
	 * @function getHasOne
	 * @param {U} unique - the unique value to compare with.
	 * @returns {boolean} Wether it existed
	 * @description - Check if a unique value exists in the current data of this Subject.
	 * @example <caption>Example check for key to exist.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 2, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data, (x) => x.key);
	 * myState.hasOne(1);
	 */
	getHasOne(unique: U): boolean {
		if (this.getUniqueMethod) {
			return this.getValue().some((x) => this.getUniqueMethod(x) === unique);
		} else {
			throw new Error('Cannot use hasOne when no unique method provided to check for uniqueness');
		}
	}

	/**
	 * @function remove
	 * @param {unknown[]} uniques - The unique values to remove.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		if (this.getUniqueMethod) {
			let next = this.getValue();
			if (!next) return this;
			uniques.forEach((unique) => {
				next = next.filter((x) => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					return this.getUniqueMethod(x) !== unique;
				});
			});

			this.setValue(next);
		}
		return this;
	}

	/**
	 * @function removeOne
	 * @param {unknown} unique - The unique value to remove.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		if (this.getUniqueMethod) {
			let next = this.getValue();
			if (!next) return this;
			next = next.filter((x) => {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				return this.getUniqueMethod(x) !== unique;
			});

			this.setValue(next);
		}
		return this;
	}

	/**
	 * @function filter
	 * @param predicate
	 * @param {unknown} filterMethod - The unique value to remove.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
	 */
	filter(predicate: (value: T, index: number, array: T[]) => boolean) {
		const value = this.getValue();
		if (value) {
			this.setValue(value.filter(predicate));
		}
		return this;
	}

	/**
	 * @function appendOne
	 * @param {T} entry - new data to be added in this Subject.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		if (this.getUniqueMethod) {
			pushToUniqueArray(next, entry, this.getUniqueMethod);
		} else {
			next.push(entry);
		}
		this.setValue(next);
		return this;
	}

	/**
	 * @function appendOneAt
	 * @param {T} entry - new data to be added in this Subject.
	 * @param {T} index - index of where to append this data into the Subject.
	 * @returns {UmbArrayState<T>} Reference to it self.
	 * @description - Append some new data to this Subject.
	 * @example <caption>Example append some data.</caption>
	 * const data = [
	 * 	{ key: 1, value: 'foo'},
	 * 	{ key: 3, value: 'bar'}
	 * ];
	 * const myState = new UmbArrayState(data);
	 * myState.appendOneAt({ key: 2, value: 'in-between'}, 1);
	 */
	appendOneAt(entry: T, index: number) {
		const next = [...this.getValue()];
		if (this.getUniqueMethod) {
			pushAtToUniqueArray(next, entry, this.getUniqueMethod, index);
		} else if (index === -1 || index >= next.length) {
			next.push(entry);
		} else {
			next.splice(index, 0, entry);
		}
		this.setValue(next);
		return this;
	}

	/**
	 * @function append
	 * @param {T[]} entries - A array of new data to be added in this Subject.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		if (this.getUniqueMethod) {
			const next = [...this.getValue()];
			entries.forEach((entry) => {
				pushToUniqueArray(next, entry, this.getUniqueMethod!);
			});
			this.setValue(next);
		} else {
			this.setValue([...this.getValue(), ...entries]);
		}
		return this;
	}

	/**
	 * @function updateOne
	 * @param {unknown} unique - Unique value to find entry to update.
	 * @param {Partial<T>} entry - new data to be added in this Subject.
	 * @returns {UmbArrayState<T>} Reference to it self.
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
		if (!this.getUniqueMethod) {
			throw new Error("Can't partial update an ArrayState without a getUnique method provided when constructed.");
		}
		this.setValue(partialUpdateFrozenArray(this.getValue(), entry, (x) => unique === this.getUniqueMethod!(x)));
		return this;
	}

	override destroy() {
		super.destroy();
		this.#sortMethod = undefined;
		(this.getUniqueMethod as unknown) = undefined;
	}
}
