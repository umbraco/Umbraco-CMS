import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import {
	CreateDictionaryItemRequestModel,
	DictionaryItemResponseModel,
	DictionaryResource,
	ImportDictionaryRequestModel,
	LanguageResource,
	UpdateDictionaryItemRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the Dictionary detail that fetches data from the server
 * @export
 * @class UmbDictionaryDetailServerDataSource
 * @implements {DictionaryDetailDataSource}
 */
export class UmbDictionaryDetailServerDataSource
	implements
		UmbDataSource<CreateDictionaryItemRequestModel, any, UpdateDictionaryItemRequestModel, DictionaryItemResponseModel>
{
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new Dictionary scaffold
	 * @param {string} parentId
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async createScaffold(parentId?: string | null, name?: string) {
		const data = {
			id: UmbId.new(),
			parentId,
			name,
			translations: [],
		};

		return { data };
	}

	/**
	 * @description - Fetches a Dictionary with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	get(id: string) {
		return tryExecuteAndNotify(this.#host, DictionaryResource.getDictionaryById({ id }));
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
	async update(id: string, dictionary: UpdateDictionaryItemRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!dictionary) throw new Error('Dictionary is missing');

		const payload = { id, requestBody: dictionary };
		return tryExecuteAndNotify(this.#host, DictionaryResource.putDictionaryById(payload));
	}

	/**
	 * @description - Inserts a new Dictionary on the server
	 * @param {DictionaryDetails} data
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async insert(data: CreateDictionaryItemRequestModel) {
		return tryExecuteAndNotify(this.#host, DictionaryResource.postDictionary({ requestBody: data }));
	}

	/**
	 * @description - Deletes a Dictionary on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return await tryExecuteAndNotify(this.#host, DictionaryResource.deleteDictionaryById({ id }));
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} temporaryFileId
	 * @param {string?} parentId
	 * @returns {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async import(temporaryFileId: string, parentId?: string) {
		// TODO => parentId will be a guid param once #13786 is merged and API regenerated
		return await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryImport({ requestBody: { temporaryFileId, parentId } })
		);
	}

	/**
	 * @description - Upload a Dictionary
	 * @param {ImportDictionaryRequestModel} formData
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async upload(formData: ImportDictionaryRequestModel) {
		return await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionaryImport({
				requestBody: formData,
			})
		);
	}

	/**
	 * @description - Export a Dictionary, optionally including child items.
	 * @param {string} id
	 * @param {boolean} includeChildren
	 * @return {*}
	 * @memberof UmbDictionaryDetailServerDataSource
	 */
	async export(id: string, includeChildren: boolean) {
		return await tryExecuteAndNotify(this.#host, DictionaryResource.getDictionaryByIdExport({ id, includeChildren }));
	}

	async getLanguages() {
		// TODO => temp until language service exists. Need languages as the dictionary response
		// includes the translated iso codes only, no friendly names and no way to tell if a dictionary
		// is missing a translation
		return tryExecuteAndNotify(this.#host, LanguageResource.getLanguage({ skip: 0, take: 1000 }));
	}
}
