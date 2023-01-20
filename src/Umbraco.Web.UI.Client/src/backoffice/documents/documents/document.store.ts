import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../../../core/stores/store';
import type { DocumentDetails } from '@umbraco-cms/models';
import { DocumentResource, DocumentTreeItem, FolderTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';

export const isDocumentDetails = (document: DocumentDetails | DocumentTreeItem): document is DocumentDetails => {
	return (document as DocumentDetails).data !== undefined;
};

export type UmbDocumentStoreItemType = DocumentDetails | DocumentTreeItem;

// TODO: research how we write names of global consts.
export const STORE_ALIAS = 'UmbDocumentStore';

/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbDocumentStoreBase<DocumentDetails | DocumentTreeItem>}
 * @description - Data Store for Documents
 */
export class UmbDocumentStore extends UmbNodeStoreBase<UmbDocumentStoreItemType> {
	public readonly storeAlias = STORE_ALIAS;

	getByKey(key: string): Observable<DocumentDetails | null> {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map(
				(documents) =>
					(documents.find((document) => document.key === key && isDocumentDetails(document)) as DocumentDetails) || null
			)
		);
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
		tryExecuteAndNotify(this.host, DocumentResource.getTreeDocumentRoot({})).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null && !item.isTrashed)));
	}

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		tryExecuteAndNotify(
			this.host,
			DocumentResource.getTreeDocumentChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		// TODO: how do we handle trashed items?
		// TODO: remove ignore when we know how to handle trashed items.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed)));
	}

	getTreeItems(keys: Array<string>): Observable<Array<FolderTreeItem>> {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this.host,
				DocumentResource.getTreeDocumentItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					this.updateItems(data);
				}
			});
		}

		return this.items.pipe(map((items) => items.filter((item) => keys.includes(item.key ?? ''))));
	}
}

export const UMB_DOCUMENT_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentStore>(STORE_ALIAS);
