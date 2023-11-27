import { UmbBasicState } from './basic-state.js';

/**
 * @export
 * @class UmbBooleanState
 * @extends {UmbBasicState<T>}
 * @description - This state ensures the data is unique, not updating any Observes unless there is an actual change of the value.
 */
export class UmbBooleanState<T> extends UmbBasicState<T | boolean> {
	constructor(initialData: T | boolean) {
		super(initialData);
	}
}
