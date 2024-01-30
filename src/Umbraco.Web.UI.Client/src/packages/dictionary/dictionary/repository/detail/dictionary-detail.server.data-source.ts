import type { UmbDictionaryDetailModel } from '../../types.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDictionaryItemRequestModel,
	DictionaryItemModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { DictionaryResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Dictionary that fetches data from the server
 * @export
 * @class UmbDictionaryServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDictionaryServerDataSource implements UmbDetailDataSource<UmbDictionaryDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Dictionary scaffold
	 * @return { CreateDictionaryRequestModel }
	 * @memberof UmbDictionaryServerDataSource
	 */
	async createScaffold() {
		const data: UmbDictionaryDetailModel = {
			entityType: UMB_DICTIONARY_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			translations: [],
		};

		return { data };
	}

	/**
	 * Fetches a Dictionary with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, DictionaryResource.getDictionaryById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const dictionary: UmbDictionaryDetailModel = {
			entityType: UMB_DICTIONARY_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			translations: data.translations,
		};

		return { data: dictionary };
	}

	/**
	 * Inserts a new Dictionary on the server
	 * @param {UmbDictionaryDetailModel} model
	 * @return {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async create(model: UmbDictionaryDetailModel) {
		if (!model) throw new Error('Dictionary is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDictionaryItemRequestModel = {
			id: model.unique,
			parentId: null,
			name: model.name,
			translations: model.translations,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.postDictionary({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Dictionary on the server
	 * @param {UmbDictionaryDetailModel} Dictionary
	 * @return {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async update(model: UmbDictionaryDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: DictionaryItemModelBaseModel = {
			name: model.name,
			translations: model.translations,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DictionaryResource.putDictionaryById({
				id: model.unique,
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Deletes a Dictionary on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DictionaryResource.deleteDictionaryById({
				id: unique,
			}),
		);
	}
}
