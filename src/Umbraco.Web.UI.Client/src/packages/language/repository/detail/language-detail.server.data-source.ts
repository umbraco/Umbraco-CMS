import type { UmbLanguageDetailModel } from '../../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type {
	CreateLanguageRequestModel,
	UpdateLanguageRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { LanguageService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Language that fetches data from the server
 * @class UmbLanguageServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbLanguageServerDataSource implements UmbDetailDataSource<UmbLanguageDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLanguageServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLanguageServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Language scaffold
	 * @param {Partial<UmbLanguageDetailModel>} [preset] Initial values for the scaffold.
	 * @returns { CreateLanguageRequestModel } The language scaffold.
	 * @memberof UmbLanguageServerDataSource
	 */
	async createScaffold(preset: Partial<UmbLanguageDetailModel> = {}) {
		const data: UmbLanguageDetailModel = {
			entityType: UMB_LANGUAGE_ENTITY_TYPE,
			fallbackIsoCode: null,
			isDefault: false,
			isMandatory: false,
			name: '',
			unique: '',
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Language with the given id from the server
	 * @param {string} unique The iso code of the language to fetch.
	 * @returns {Promise<UmbDataSourceResponse<UmbLanguageDetailModel>>} The language.
	 * @memberof UmbLanguageServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbLanguageDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			LanguageService.getLanguageByIsoCode({ path: { isoCode: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const dataType: UmbLanguageDetailModel = {
			entityType: UMB_LANGUAGE_ENTITY_TYPE,
			fallbackIsoCode: data.fallbackIsoCode || null,
			isDefault: data.isDefault,
			isMandatory: data.isMandatory,
			name: data.name,
			unique: data.isoCode,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Language on the server
	 * @param {UmbLanguageDetailModel} model The language to create.
	 * @returns {Promise<UmbDataSourceResponse<UmbLanguageDetailModel>>} The created language.
	 * @memberof UmbLanguageServerDataSource
	 */
	async create(model: UmbLanguageDetailModel): Promise<UmbDataSourceResponse<UmbLanguageDetailModel>> {
		if (!model) throw new Error('Language is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateLanguageRequestModel = {
			fallbackIsoCode: model.fallbackIsoCode,
			isDefault: model.isDefault,
			isMandatory: model.isMandatory,
			isoCode: model.unique,
			name: model.name,
		};

		const { data, error } = await tryExecute(
			this.#host,
			LanguageService.postLanguage({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Language on the server
	 * @param {UmbLanguageDetailModel} model The language to update.
	 * @returns {Promise<UmbDataSourceResponse<UmbLanguageDetailModel>>} The updated language.
	 * @memberof UmbLanguageServerDataSource
	 */
	async update(model: UmbLanguageDetailModel): Promise<UmbDataSourceResponse<UmbLanguageDetailModel>> {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateLanguageRequestModel = {
			fallbackIsoCode: model.fallbackIsoCode,
			isDefault: model.isDefault,
			isMandatory: model.isMandatory,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			LanguageService.putLanguageByIsoCode({
				path: { isoCode: model.unique },
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Language on the server
	 * @param {string} unique The iso code of the language to delete.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbLanguageServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			LanguageService.deleteLanguageByIsoCode({
				path: { isoCode: unique },
			}),
		);
	}
}
