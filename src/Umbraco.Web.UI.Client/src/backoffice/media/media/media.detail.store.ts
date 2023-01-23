import type { DocumentDetails, MediaDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContentStore } from '@umbraco-cms/stores/store';


export const UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaDetailStore>('UmbDocumentDetailStore');


/**
 * @export
 * @class UmbMediaStore
 * @extends {UmbStoreBase<DocumentDetails>}
 * @description - Data Store for Media
 */
export class UmbMediaDetailStore extends UmbStoreBase implements UmbContentStore<MediaDetails> {


	private _data = new UniqueArrayBehaviorSubject<DocumentDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/media/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this._data.append(data);
			});

		return createObservablePart(this._data, (documents) =>
			documents.find((document) => document.key === key)
		);
	}

	// TODO: make sure UI somehow can follow the status of this action.
	save(data: MediaDetails[]) {
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
		return fetch('/umbraco/management/api/v1/media/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<MediaDetails>) => {
				this._data.append(data);
			});
	}

	// TODO: how do we handle trashed items?
	// TODO: How do we make trash available on details and tree store?
	async trash(keys: Array<string>) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/media/trash', {
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
