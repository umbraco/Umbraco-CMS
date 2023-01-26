import type { Observable } from 'rxjs';
import { MediaResource, ContentTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');

// TODO: Stop using ContentTreeItem
export type MediaTreeItem = ContentTreeItem;

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media
 */
export class UmbMediaTreeStore extends UmbStoreBase {
	#data = new ArrayState<MediaTreeItem>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: how do we handle trashed items?
	// TODO: How do we make trash available on details and tree store?
	async trash(keys: Array<string>) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/media/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.#data.append(data);
	}

	async move(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/media/move', {
			method: 'POST',
			body: JSON.stringify({ keys, destination }),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.#data.append(data);
	}

	getTreeRoot(): Observable<Array<MediaTreeItem>> {
		tryExecuteAndNotify(this._host, MediaResource.getTreeMediaRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this.#data, (items) =>
			items.filter((item) => item.parentKey === null && !item.isTrashed)
		);
	}

	getTreeItemChildren(key: string): Observable<Array<MediaTreeItem>> {
		tryExecuteAndNotify(
			this._host,
			MediaResource.getTreeMediaChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this.#data, (items) =>
			items.filter((item) => item.parentKey === key && !item.isTrashed)
		);
	}

	getTreeItems(keys: Array<string>): Observable<Array<MediaTreeItem>> {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				MediaResource.getTreeMediaItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this.#data.append(data);
				}
			});
		}

		return createObservablePart(this.#data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
