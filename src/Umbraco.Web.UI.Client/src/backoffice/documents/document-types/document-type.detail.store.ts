import type { DocumentTypeDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeDetailStore>('UmbDocumentTypeDetailStore');


/**
 * @export
 * @class UmbDocumentTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Document Types
 */
export class UmbDocumentTypeDetailStore extends UmbStoreBase {


	#data = new UniqueArrayBehaviorSubject<DocumentTypeDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DocumentTypeDetails | undefined>)}
	 * @memberof UmbDocumentTypesStore
	 */
	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document/document-type/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.append(data);
			});

		return createObservablePart(this.#data, (documentTypes) =>
			documentTypes.find((documentType) => documentType.key === key)
		);
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a Data Type.
	 * @param {Array<DocumentTypeDetails>} documentTypes
	 * @memberof UmbDocumentTypesStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: DocumentTypeDetails[]) {
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
		return fetch('/umbraco/management/api/v1/document-type/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DocumentTypeDetails>) => {
				this.#data.append(data);
			});
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDocumentTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/document-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}
}
