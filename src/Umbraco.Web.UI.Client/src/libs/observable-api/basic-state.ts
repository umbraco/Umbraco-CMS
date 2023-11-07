import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @export
 * @class UmbBasicState
 * @description - State ensures the data is unique, not updating any Observes unless there is an actual change of the value using `===`.
 */
export class UmbBasicState<T> {

	protected _subject:BehaviorSubject<T>;

	constructor(initialData: T) {
		this._subject = new BehaviorSubject(initialData);
		this.asObservable = this._subject.asObservable;
		this.getValue = this._subject.getValue;
		this.complete = this._subject.complete;
	}

	/**
	 *
	 */
	public asObservable: BehaviorSubject<T>['asObservable'];
	public get value(): BehaviorSubject<T>['value'] {
		return this._subject.value;
	};
	public getValue: BehaviorSubject<T>['getValue'];
	public complete: BehaviorSubject<T>['complete'];


	next(newData: T): void {
		if (newData !== this._subject.getValue()) {
			this._subject.next(newData);
		}
	}
}
