import type { DocumentDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase, UmbContentStore } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentDetailStore>('UmbDocumentDetailStore');


/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Documents
 */
export class UmbDocumentDetailStore extends UmbStoreBase implements UmbContentStore<DocumentDetails> {


	private _data = new ArrayState<DocumentDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this._data.append(data);
			});

		return createObservablePart(this._data, (documents) =>
			documents.find((document) => document.key === key)
		);
	}

	// TODO: make sure UI somehow can follow the status of this action.
	save(data: DocumentDetails[]) {
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
				this._data.append(data);
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
		this._data.append(data);
	}
}
