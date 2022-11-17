import { map, Observable } from 'rxjs';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import type { DocumentDetails } from '@umbraco-cms/models';
import { ApiError, DocumentResource, DocumentTreeItem, FolderTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbDocumentStoreBase<DataTypeDetails | DocumentTreeItem>}
 * @description - Data Store for Documents
 */
export class UmbDocumentStore extends UmbDataStoreBase<DocumentDetails | DocumentTreeItem> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

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
}
