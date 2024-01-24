import { createObservablePart, type MappingFunction, type MemoizationFunction } from '../index.js';
import { UmbBasicState } from './basic-state.js';

/**
 * @export
 * @class UmbStringState
 * @extends {UmbBasicState<T>}
 * @description - A state holding string data, this ensures the data is unique, not updating any Observes unless there is an actual change of the value, by using `===`.
 */
export class UmbStringState<T> extends UmbBasicState<T | string> {
	constructor(initialData: T | string) {
		super(initialData);
	}

	asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T | string, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	) {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction);
	}
}
