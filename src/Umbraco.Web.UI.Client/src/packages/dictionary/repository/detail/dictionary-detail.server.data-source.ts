import type { UmbDictionaryDetailModel } from '../../types.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDictionaryItemRequestModel,
	UpdateDictionaryItemRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Dictionary that fetches data from the server
 * @class UmbDictionaryServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDictionaryServerDataSource implements UmbDetailDataSource<UmbDictionaryDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDictionaryServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Dictionary scaffold
	 * @returns { CreateDictionaryRequestModel }
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
	 * @returns {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, DictionaryService.getDictionaryById({ id: unique }));

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
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async create(model: UmbDictionaryDetailModel, parentUnique: string | null) {
		if (!model) throw new Error('Dictionary is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateDictionaryItemRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
			translations: model.translations,
		};

		const { data, error } = await tryExecute(
			this.#host,
			DictionaryService.postDictionary({
				body,
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
	 * @param model
	 * @returns {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async update(model: UmbDictionaryDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateDictionaryItemRequestModel = {
			name: model.name,
			translations: model.translations,
		};

		const { error } = await tryExecute(
			this.#host,
			DictionaryService.putDictionaryById({
				id: model.unique,
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Dictionary on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDictionaryServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			DictionaryService.deleteDictionaryById({
				id: unique,
			}),
		);
	}
}
