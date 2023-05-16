import {
	LanguageResource,
	LanguageResponseModel,
	CreateLanguageRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Language that fetches data from the server
 * @export
 * @class UmbLanguageServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbLanguageServerDataSource
	implements UmbDataSource<CreateLanguageRequestModel, any, any, LanguageResponseModel>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbLanguageServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbLanguageServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Language with the given iso code from the server
	 * @param {string} isoCode
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async get(isoCode: string) {
		if (!isoCode) throw new Error('Iso Code is missing');
		return tryExecuteAndNotify(
			this.#host,
			LanguageResource.getLanguageByIsoCode({
				isoCode,
			})
		);
	}

	/**
	 * Creates a new Language scaffold
	 * @param
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async createScaffold() {
		const data: LanguageResponseModel = {
			name: '',
			isDefault: false,
			isMandatory: false,
			isoCode: '',
		};

		return { data };
	}

	/**
	 * Inserts a new Language on the server
	 * @param {LanguageResponseModel} language
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async insert(language: CreateLanguageRequestModel) {
		if (!language.isoCode) {
			throw new Error('Language iso code is missing');
		}

		return tryExecuteAndNotify(this.#host, LanguageResource.postLanguage({ requestBody: language }));
	}

	/**
	 * Updates a Language on the server
	 * @param {LanguageResponseModel} language
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async update(iscoCode: string, language: LanguageResponseModel) {
		if (!language.isoCode) {
			throw new Error('Language iso code is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			LanguageResource.putLanguageByIsoCode({ isoCode: language.isoCode, requestBody: language })
		);
	}

	/**
	 * Deletes a Language on the server
	 * @param {string} isoCode
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async delete(isoCode: string) {
		if (!isoCode) throw new Error('Iso Code is missing');
		return tryExecuteAndNotify(this.#host, LanguageResource.deleteLanguageByIsoCode({ isoCode }));
	}

	/**
	 * Get a list of Languages on the server
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async getCollection({ skip, take }: { skip: number; take: number }) {
		return tryExecuteAndNotify(this.#host, LanguageResource.getLanguage({ skip, take }));
	}
}
