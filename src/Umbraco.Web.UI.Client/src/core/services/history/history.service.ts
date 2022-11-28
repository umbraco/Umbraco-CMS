import { BehaviorSubject, Observable } from 'rxjs';

export type UmbModelType = 'dialog' | 'sidebar';

export type UmbHistoryItem = {
	path: string;
	icon?: string;
};

export class UmbHistoryService {
	private _history: BehaviorSubject<Array<UmbHistoryItem>> = new BehaviorSubject(<Array<UmbHistoryItem>>[]);
	public readonly history: Observable<Array<UmbHistoryItem>> = this._history.asObservable();

	/**
	 * Pushes a new history item to the history array
	 * @public
	 * @param {UmbHistoryItem} historyItem
	 * @memberof UmbHistoryService
	 */
	public push(historyItem: UmbHistoryItem): void {
		this._history.next([...this._history.getValue(), historyItem]);
	}

	/**
	 * Clears the history array
	 * @public
	 * @memberof UmbHistoryService
	 */
	public clear() {
		this._history.next([]);
	}
}
