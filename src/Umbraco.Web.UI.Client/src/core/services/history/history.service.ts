import { BehaviorSubject, Observable } from 'rxjs';

export type UmbModelType = 'dialog' | 'sidebar';

export type UmbHistoryItem = {
	path: string;
	label: string | Array<string>;
	icon?: string;
};

class UmbHistoryService {
	private _history: BehaviorSubject<Array<UmbHistoryItem>> = new BehaviorSubject(<Array<UmbHistoryItem>>[
		{ label: 'Users grid', path: 'section/users/view/users/overview/grid' },
		{ label: ['User', 'Nat Linnane'], path: 'section/users/view/users/user/50f184d4-71f3-4a43-b8be-7a36340fbd0d' },
		{
			label: ['User Group', 'Administrator'],
			path: 'section/users/view/users/userGroup/10000000-0000-0000-0000-000000000000',
		},
		{ label: 'About us', path: 'section/content/document/74e4008a-ea4f-4793-b924-15e02fd380d2/view/content' },
		{
			label: ['Blog', 'Look at this nice history page!'],
			path: 'section/content/document/74e4008a-ea4f-4793-b924-15e02fd380d2/view/content',
		},
	]);
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

export const umbHistoryService = new UmbHistoryService();
