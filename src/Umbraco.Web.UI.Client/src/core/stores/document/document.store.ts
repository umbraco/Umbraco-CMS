import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../store';
import type { DocumentDetails } from '@umbraco-cms/models';
import { ApiError, DocumentResource, DocumentTreeItem, FolderTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

const isDocumentDetails = (document: DocumentDetails | DocumentTreeItem): document is DocumentDetails => {
	return (document as DocumentDetails).data !== undefined;
};

export type UmbDocumentStoreItemType = DocumentDetails | DocumentTreeItem

/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbDocumentStoreBase<DocumentDetails | DocumentTreeItem>}
 * @description - Data Store for Documents
 */
export class UmbDocumentStore extends UmbNodeStoreBase<UmbDocumentStoreItemType> {

	public readonly storeAlias = 'umbDocumentStore';

	getByKey(key: string): Observable<DocumentDetails | null> {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});
			
		return this.items.pipe(map((documents) => documents.find((document) => document.key === key && isDocumentDetails(document)) as DocumentDetails || null));
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

		// TODO: use backend cli when available.
		return fetch('/umbraco/management/api/v1/document/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DocumentDetails>) => {
				this.updateItems(data);
			});
	}

	// TODO: how do we handle trashed items?
	async trash(keys: Array<string>) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/document/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.updateItems(data);
	}

	getTreeRoot(): Observable<Array<DocumentTreeItem>> {
		DocumentResource.getTreeDocumentRoot({}).then(
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

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		DocumentResource.getTreeDocumentChildren({
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

	getTreeItems(keys: Array<string>): Observable<Array<FolderTreeItem>> {
		if (keys.length > 0) {
			DocumentResource.getTreeDocumentItem({
				key: keys,
			}).then(
				(items) => {
					this.updateItems(items);
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
