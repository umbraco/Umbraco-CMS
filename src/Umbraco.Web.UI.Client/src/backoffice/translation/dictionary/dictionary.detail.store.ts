import type { DictionaryDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { EntityTreeItem } from '@umbraco-cms/backend-api';


export const UMB_DICTIONARY_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryDetailStore>('UmbDictionaryDetailStore');


/**
 * @export
 * @class UmbDictionaryDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Data Types
 */
export class UmbDictionaryDetailStore extends UmbStoreBase {


	// TODO: use the right type:
	#data = new UniqueArrayBehaviorSubject<EntityTreeItem>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DICTIONARY_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DictionaryDetails | undefined>)}
	 * @memberof UmbDictionaryDetailStore
	 */
	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document/dictionary/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.append(data);
			});

		return createObservablePart(this.#data, (documents) =>
			documents.find((document) => document.key === key)
		);
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a Dictionary.
	 * @param {Array<DictionaryDetails>} Dictionaries
	 * @memberof UmbDictionaryDetailStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: DictionaryDetails[]) {
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
		return fetch('/umbraco/management/api/v1/dictionary/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DictionaryDetails>) => {
				this.#data.append(data);
			});
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDictionaryDetailStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/dictionary/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}
}
