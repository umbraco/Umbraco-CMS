import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import type { MediaDetails } from '@umbraco-cms/models';
import { ApiError, ContentTreeItem, MediaResource, ProblemDetails } from '@umbraco-cms/backend-api';

const isMediaDetails = (media: UmbMediaStoreItemType): media is MediaDetails => {
	return (media as MediaDetails).data !== undefined;
};

// TODO: stop using ContentTreeItem.
export type UmbMediaStoreItemType = MediaDetails | ContentTreeItem;

/**
 * @export
 * @class UmbMediaStore
 * @extends {UmbMediaStoreBase<MediaDetails | MediaTreeItem>}
 * @description - Data Store for Media
 */
export class UmbMediaStore extends UmbDataStoreBase<UmbMediaStoreItemType> {

	public readonly storeAlias = 'umbMediaStore';

	getByKey(key: string): Observable<MediaDetails | null> {
		// fetch from server and update store
		fetch(`/umbraco/management/api/v1/media/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});
			
		return this.items.pipe(map((media) => media.find((media) => media.key === key && isMediaDetails(media)) as MediaDetails || null));
	}

	// TODO: make sure UI somehow can follow the status of this action.
	save(data: MediaDetails[]): Promise<void> {
		// fetch from server and update store
		// TODO: use Fetcher API.
		let body: string;

		try {
			body = JSON.stringify(data);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}

		// TODO: Use node type to hit the right API, or have a general Node API?
		return fetch('/umbraco/management/api/v1/media/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<MediaDetails>) => {
				this.updateItems(data);
			});
	}

		// TODO: how do we handle trashed items?
		async trash(keys: Array<string>) {
			// fetch from server and update store
			// TODO: Use node type to hit the right API, or have a general Node API?
			const res = await fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify(keys),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const data = await res.json();
			this.updateItems(data);
		}

	getTreeRoot(): Observable<Array<ContentTreeItem>> {
		MediaResource.getTreeMediaRoot({}).then(
			(res) => {
				this.updateItems(res.items);
			},
			(e) => {
				if (e instanceof ApiError) {
					const error = e.body as ProblemDetails;
					if (e.status === 400) {
						console.log(error.detail);
					}
				}
			}
		);
		
		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null && !item.isTrashed)));
	}

	getTreeItemChildren(key: string): Observable<Array<ContentTreeItem>> {
		MediaResource.getTreeMediaChildren({
			parentKey: key,
		}).then(
			(res) => {
				this.updateItems(res.items);
			},
			(e) => {
				if (e instanceof ApiError) {
					const error = e.body as ProblemDetails;
					if (e.status === 400) {
						console.log(error.detail);
					}
				}
			}
		);
		
		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed)));
	}
}
