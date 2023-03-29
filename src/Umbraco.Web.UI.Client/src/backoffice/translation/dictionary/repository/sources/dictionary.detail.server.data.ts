import type { DictionaryDetails } from '../../';
import { DictionaryDetailDataSource } from './dictionary.details.server.data.interface';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import {
	CreateDictionaryItemRequestModel,
	DictionaryResource,
	LanguageResource,
	ProblemDetailsModel,
} from '@umbraco-cms/backoffice/backend-api';

/**
 * @description - A data source for the Dictionary detail that fetches data from the server
 * @export
 * @class UmbDictionaryDetailServerDataSource
 * @implements {DictionaryDetailDataSource}
 */
export class UmbDictionaryDetailServerDataSource implements DictionaryDetailDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new Dictionary scaffold
	 * @param {string} parentKey
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async createScaffold(parentKey: string) {
		const data: DictionaryDetails = {
			name: '',
			parentKey,
		} as DictionaryDetails;

		return { data };
	}

	/**
	 * @description - Fetches a Dictionary with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	get(key: string) {
		return tryExecuteAndNotify(this.#host, DictionaryResource.getDictionaryByKey({ key })) as any;
	}

	/**
	 * @description - Get the dictionary overview
	 * @param {number?} skip
	 * @param {number?} take
	 * @returns {*}
	 */
	list(skip = 0, take = 1000) {
		return tryExecuteAndNotify(this.#host, DictionaryResource.getDictionary({ skip, take }));
	}

	/**
	 * @description - Updates a Dictionary on the server
	 * @param {DictionaryDetails} dictionary
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async update(dictionary: DictionaryDetails) {
		if (!dictionary.key) {
			const error: ProblemDetailsModel = { title: 'Dictionary key is missing' };
			return { error };
		}

		const payload = { key: dictionary.key, requestBody: dictionary };
		return tryExecuteAndNotify(this.#host, DictionaryResource.putDictionaryByKey(payload));
	}

	/**
	 * @description - Inserts a new Dictionary on the server
	 * @param {DictionaryDetails} data
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async insert(data: DictionaryDetails) {
		const requestBody: CreateDictionaryItemRequestModel = {
			parentKey: data.parentKey,
			name: data.name,
		};

		// TODO: fix type mismatch:
		return tryExecuteAndNotify(this.#host, DictionaryResource.postDictionary({ requestBody })) as any;
	}

	/**
	 * @description - Deletes a Dictionary on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return await tryExecuteAndNotify(this.#host, DictionaryResource.deleteDictionaryByKey({ key }));
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} fileName
	 * @param {string?} parentKey
	 * @returns {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async import(fileName: string, parentKey?: string) {
		// TODO => parentKey will be a guid param once #13786 is merged and API regenerated
		return await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryImport({ requestBody: { fileName, parentKey } })
		);
	}

	/**
	 * @description - Upload a Dictionary
	 * @param {FormData} formData
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async upload(formData: FormData) {
		return await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryUpload({
				requestBody: formData,
			})
		);
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} key
	 * @param {boolean} includeChildren
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async export(key: string, includeChildren: boolean) {
		return await tryExecuteAndNotify(this.#host, DictionaryResource.getDictionaryByKeyExport({ key, includeChildren }));
	}

	async getLanguages() {
		// TODO => temp until language service exists. Need languages as the dictionary response
		// includes the translated iso codes only, no friendly names and no way to tell if a dictionary
		// is missing a translation
		return await tryExecuteAndNotify(this.#host, LanguageResource.getLanguage({ skip: 0, take: 1000 }));
	}
}
