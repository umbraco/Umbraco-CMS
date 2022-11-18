import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import type { DocumentDetails } from '@umbraco-cms/models';
import { ApiError, DocumentResource, DocumentTreeItem, FolderTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbDocumentStoreBase<DocumentDetails | DocumentTreeItem>}
 * @description - Data Store for Documents
 */
export class UmbDocumentStore extends UmbDataStoreBase<DocumentDetails | DocumentTreeItem> {
	getTreeRoot(): Observable<Array<DocumentTreeItem>> {
		DocumentResource.getTreeDocumentRoot({}).then(
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

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		DocumentResource.getTreeDocumentChildren({
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

	getTreeItems(keys: Array<string>): Observable<Array<FolderTreeItem>> {
		if (keys.length > 0) {
			DocumentResource.getTreeDocumentItem({
				key: keys,
			}).then(
				(items) => {
					this.update(items);
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
		}

		return this.items.pipe(map((items) => items.filter((item) => keys.includes(item.key ?? ''))));
	}
}
