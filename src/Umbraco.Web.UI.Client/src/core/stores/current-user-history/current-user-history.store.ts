import { BehaviorSubject, Observable } from 'rxjs';

export type UmbModelType = 'dialog' | 'sidebar';

export type UmbCurrentUserHistoryItem = {
	path: string;
	label: string | Array<string>;
	icon?: string;
};

export class UmbCurrentUserHistoryStore {

	private _history: BehaviorSubject<Array<UmbCurrentUserHistoryItem>> = new BehaviorSubject(<Array<UmbCurrentUserHistoryItem>>[]);
	public readonly history: Observable<Array<UmbCurrentUserHistoryItem>> = this._history.asObservable();

	/**
	 * Pushes a new history item to the history array
	 * @public
	 * @param {UmbCurrentUserHistoryItem} historyItem
	 * @memberof UmbHistoryService
	 */
	public push(historyItem: UmbCurrentUserHistoryItem): void {
		const history = this._history.getValue();
		const lastItem = history[history.length - 1];

		// This prevents duplicate entries in the history array.
		if (!lastItem || lastItem.path !== historyItem.path) {
			this._history.next([...this._history.getValue(), historyItem]);
		} else {
			//Update existing item
			const newHistory = this._history.getValue();
			newHistory[history.length - 1] = historyItem;
			this._history.next(newHistory);
		}
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
