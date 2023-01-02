import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import { ApiError, DocumentTypeResource, DocumentTypeTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';
import type { DocumentTypeDetails } from '@umbraco-cms/models';

const isDocumentTypeDetails = (documentType: DocumentTypeDetails | DocumentTypeTreeItem): documentType is DocumentTypeDetails => {
	return (documentType as DocumentTypeDetails).properties !== undefined;
};

export type UmbDocumentTypeStoreItemType = DocumentTypeDetails | DocumentTypeTreeItem;

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbDataStoreBase<DocumentTypeDetails | DocumentTypeTreeItem>}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeStore extends UmbDataStoreBase<UmbDocumentTypeStoreItemType> {

	public readonly storeAlias = 'umbDocumentTypeStore';

	getByKey(key: string): Observable<DocumentTypeDetails | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/document-type/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(
			map((documentTypes) => documentTypes.find((documentType) => documentType.key === key && isDocumentTypeDetails(documentType)) as DocumentTypeDetails || null)
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
		DocumentTypeResource.getTreeDocumentTypeRoot({}).then(
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

	getTreeItemChildren(key: string): Observable<Array<DocumentTypeTreeItem>> {
		DocumentTypeResource.getTreeDocumentTypeChildren({
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
