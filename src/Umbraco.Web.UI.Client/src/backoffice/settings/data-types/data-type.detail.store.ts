import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDataTypeDetailStore>('UmbDataTypeDetailStore');


/**
 * @export
 * @class UmbDataTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Data Types
 */
export class UmbDataTypeDetailStore extends UmbStoreBase implements UmbEntityDetailStore<DataTypeDetails> {


	#data = new ArrayState<DataTypeDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}


	getScaffold(entityType: string, parentKey: string | null) {
		return {
			key: '',
			name: '',
			icon: '',
			type: 'data-type',
			hasChildren: false,
			parentKey: '',
			propertyEditorModelAlias: '',
			propertyEditorUIAlias: '',
			data: [],
		} as DataTypeDetails;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | undefined>)}
	 * @memberof UmbDataTypesStore
	 */
	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/backoffice/data-type/details/${key}`)
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
	 * @description - Save a Data Type.
	 * @param {Array<DataTypeDetails>} dataTypes
	 * @memberof UmbDataTypesStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: DataTypeDetails[]) {
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
		return fetch('/umbraco/management/api/v1/data-type/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DataTypeDetails>) => {
				this.#data.append(data);
			});
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDataTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/data-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}
}
