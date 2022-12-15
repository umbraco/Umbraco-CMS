import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import { MediaTypeResource, ApiError, ProblemDetails, FolderTreeItem } from '@umbraco-cms/backend-api';
import type { MediaTypeDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMediaTypeStore
 * @extends {UmbDataStoreBase<MediaTypeDetails | EntityTreeItem>}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeStore extends UmbDataStoreBase<MediaTypeDetails | FolderTreeItem> {
	getTreeRoot(): Observable<Array<FolderTreeItem>> {
		MediaTypeResource.getTreeMediaTypeRoot({}).then(
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

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		MediaTypeResource.getTreeMediaTypeChildren({
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

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
