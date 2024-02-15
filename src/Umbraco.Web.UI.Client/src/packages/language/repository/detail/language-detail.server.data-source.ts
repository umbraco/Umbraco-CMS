import type { UmbLanguageDetailModel } from '../../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateLanguageRequestModel, LanguageModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LanguageResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Language that fetches data from the server
 * @export
 * @class UmbLanguageServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbLanguageServerDataSource implements UmbDetailDataSource<UmbLanguageDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLanguageServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbLanguageServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Language scaffold
	 * @param {Partial<UmbLanguageDetailModel>} [preset]
	 * @return { CreateLanguageRequestModel }
	 * @memberof UmbLanguageServerDataSource
	 */
	async createScaffold(preset: Partial<UmbLanguageDetailModel> = {}) {
		const data: UmbLanguageDetailModel = {
			entityType: UMB_LANGUAGE_ENTITY_TYPE,
			fallbackIsoCode: null,
			isDefault: false,
			isMandatory: false,
			name: '',
			unique: UmbId.new(), // Creating a temporary unique until the culture is selected
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Language with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			LanguageResource.getLanguageByIsoCode({ isoCode: unique }),
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
	 * @param {UmbLanguageDetailModel} model
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async create(model: UmbLanguageDetailModel) {
		if (!model) throw new Error('Language is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateLanguageRequestModel = {
			fallbackIsoCode: model.fallbackIsoCode,
			isDefault: model.isDefault,
			isMandatory: model.isMandatory,
			isoCode: model.unique,
			name: model.name,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			LanguageResource.postLanguage({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Language on the server
	 * @param {UmbLanguageDetailModel} Language
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async update(model: UmbLanguageDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: LanguageModelBaseModel = {
			fallbackIsoCode: model.fallbackIsoCode,
			isDefault: model.isDefault,
			isMandatory: model.isMandatory,
			name: model.name,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			LanguageResource.putLanguageByIsoCode({
				isoCode: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Language on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			LanguageResource.deleteLanguageByIsoCode({
				isoCode: unique,
			}),
		);
	}
}
