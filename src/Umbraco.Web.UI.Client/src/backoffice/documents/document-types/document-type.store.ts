import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '@umbraco-cms/store';
import { DocumentTypeResource, DocumentTypeTreeItem } from '@umbraco-cms/backend-api';
import type { DocumentTypeDetails } from '@umbraco-cms/models';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';

export const isDocumentTypeDetails = (
	documentType: DocumentTypeDetails | DocumentTypeTreeItem
): documentType is DocumentTypeDetails => {
	return (documentType as DocumentTypeDetails).properties !== undefined;
};

export type UmbDocumentTypeStoreItemType = DocumentTypeDetails | DocumentTypeTreeItem;

export const STORE_ALIAS = 'UmbDocumentTypeStore';

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbDataStoreBase<DocumentTypeDetails | DocumentTypeTreeItem>}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeStore extends UmbDataStoreBase<UmbDocumentTypeStoreItemType> {
	public readonly storeAlias = STORE_ALIAS;

	getByKey(key: string): Observable<DocumentTypeDetails | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/document-type/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map(
				(documentTypes) =>
					(documentTypes.find(
						(documentType) => documentType.key === key && isDocumentTypeDetails(documentType)
					) as DocumentTypeDetails) || null
			)
		);
	}

	async save(documentTypes: Array<DocumentTypeDetails>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/document-type/save', {
				method: 'POST',
				body: JSON.stringify(documentTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.updateItems(json);
		} catch (error) {
			console.error('Save Document Type error', error);
		}
	}

	getTreeRoot(): Observable<Array<DocumentTypeTreeItem>> {
		tryExecuteAndNotify(this.host, DocumentTypeResource.getTreeDocumentTypeRoot({})).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}

	getTreeItemChildren(key: string): Observable<Array<DocumentTypeTreeItem>> {
		tryExecuteAndNotify(
			this.host,
			DocumentTypeResource.getTreeDocumentTypeChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}

export const UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeStore>(STORE_ALIAS);
