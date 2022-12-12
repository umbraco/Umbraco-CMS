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
	getByKey(key: string): Observable<DocumentDetails | null> {
		// fetch from server and update store
		fetch(`/umbraco/management/api/v1/document/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(map((documents) => documents.find((document) => document.key === key) || null));
	}

	async trash(keys: Array<string>) {
		// fetch from server and update store
		// TODO: Use node type to hit the right API, or have a general Node API?
		const res = await fetch('/umbraco/management/api/v1/document/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.update(data);
	}

	// TODO: make sure UI somehow can follow the status of this action.
	save(data: DocumentDetails[]): Promise<void> {
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
		return fetch('/umbraco/management/api/v1/document/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DocumentDetails>) => {
				this.update(data);
			});
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
