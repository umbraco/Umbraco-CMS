import { map, Observable } from 'rxjs';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import type { MediaDetails } from '@umbraco-cms/models';
import { ApiError, ContentTreeItem, MediaResource, ProblemDetails } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbMediaStore
 * @extends {UmbMediaStoreBase<MediaDetails | MediaTreeItem>}
 * @description - Data Store for Media
 */
export class UmbMediaStore extends UmbDataStoreBase<MediaDetails | ContentTreeItem> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getTreeRoot(): Observable<Array<ContentTreeItem>> {
		MediaResource.getTreeMediaRoot({}).then(
			(res) => {
				this.update(res.items);
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

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}

	getTreeItemChildren(key: string): Observable<Array<ContentTreeItem>> {
		MediaResource.getTreeMediaChildren({
			parentKey: key,
		}).then(
			(res) => {
				this.update(res.items);
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

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
