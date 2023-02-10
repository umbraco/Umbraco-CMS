import type { DictionaryDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { EntityTreeItemModel } from '@umbraco-cms/backend-api';

export const UMB_DICTIONARY_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryDetailStore>(
	'UmbDictionaryDetailStore'
);

/**
 * @export
 * @class UmbDictionaryDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Data Types
 */
// TODO: use the right type for dictionary:
export class UmbDictionaryDetailStore extends UmbStoreBase implements UmbEntityDetailStore<EntityTreeItemModel> {
	// TODO: use the right type:
	#data = new ArrayState<EntityTreeItemModel>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DICTIONARY_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getScaffold(entityType: string, parentKey: string | null) {
		return {} as EntityTreeItemModel;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DictionaryDetails | undefined>)}
	 * @memberof UmbDictionaryDetailStore
	 */
	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/dictionary/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.append(data);
			});

		return this.#data.getObservablePart((documents) => documents.find((document) => document.key === key));
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
