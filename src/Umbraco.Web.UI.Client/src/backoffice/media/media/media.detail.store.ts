import type { DocumentDetails, MediaDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase, UmbContentStore } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaDetailStore>('UmbMediaDetailStore');


/**
 * @export
 * @class UmbMediaDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media
 */
export class UmbMediaDetailStore extends UmbStoreBase implements UmbContentStore<MediaDetails> {


	#data = new ArrayState<MediaDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/media/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.append(data);
			});

		return createObservablePart(this.#data, (documents) =>
			documents.find((document) => document.key === key)
		);
	}

	getScaffold(entityType: string, parentKey: string | null) {
		return {
			key: '',
			name: '',
			icon: '',
			type: '',
			hasChildren: false,
			parentKey: '',
			isTrashed: false,
			properties: [
				{
					alias: '',
					label: '',
					description: '',
					dataTypeKey: '',
				},
			],
			data: [
				{
					alias: '',
					value: '',
				},
			]
		} as MediaDetails;
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
				this.#data.append(data);
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
		this.#data.append(data);
	}
}
