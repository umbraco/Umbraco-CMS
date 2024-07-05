import { UMB_CURRENT_USER_HISTORY_STORE_CONTEXT } from './current-user-history.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

export type UmbModelType = 'dialog' | 'sidebar';

export type UmbCurrentUserHistoryItem = {
	unique: string;
	path: string;
	label: string | Array<string>;
	icon?: string;
};

export class UmbCurrentUserHistoryStore extends UmbStoreBase<UmbCurrentUserHistoryItem> {
	public readonly history = this._data.asObservable();
	public readonly latestHistory = this._data.asObservablePart((historyItems) => historyItems.slice(-10));

	constructor(host: UmbControllerHost) {
		super(
			host,
			UMB_CURRENT_USER_HISTORY_STORE_CONTEXT.toString(),
			new UmbArrayState<UmbCurrentUserHistoryItem>([], (x) => x.unique),
		);
		if (!('navigation' in window)) return;
		(window as any).navigation.addEventListener('navigate', (event: any) => {
			const url = new URL(event.destination.url);
			const historyItem = {
				unique: new UmbId().toString(),
				path: url.pathname,
				label: event.destination.url.split('/').pop(),
			};
			this.push(historyItem);
		});
	}

	/**
	 * Pushes a new history item to the history array
	 * @public
	 * @param {UmbCurrentUserHistoryItem} historyItem
	 * @memberof UmbHistoryService
	 */
	public push(historyItem: UmbCurrentUserHistoryItem): void {
		const history = this._data.getValue();
		const lastItem = history[history.length - 1];

		// This prevents duplicate entries in the history array.
		if (!lastItem || lastItem.path !== historyItem.path) {
			this._data.setValue([...this._data.getValue(), historyItem]);
		}
	}

	/**
	 * Clears the history array
	 * @public
	 * @memberof UmbHistoryService
	 */
	public clear() {
		this._data.setValue([]);
	}
}

// Default export for the globalContext manifest:
export default UmbCurrentUserHistoryStore;
