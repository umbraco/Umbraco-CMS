import { ProblemDetailsModel, LanguageResource, LanguageModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Language that fetches data from the server
 * @export
 * @class UmbLanguageServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbLanguageServerDataSource implements UmbLanguageServerDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbLanguageServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbLanguageServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches a Language with the given iso code from the server
	 * @param {string} isoCode
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async get(isoCode: string) {
		if (!isoCode) {
			const error: ProblemDetailsModel = { title: 'Iso Code is missing' };
			return { error };
		}

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
		const data: LanguageModel = {
			name: '',
			isDefault: false,
			isMandatory: false,
			fallbackIsoCode: '',
			isoCode: '',
		};

		return { data };
	}

	/**
	 * Inserts a new Language on the server
	 * @param {LanguageModel} language
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async insert(language: LanguageModel) {
		if (!language.isoCode) {
			const error: ProblemDetailsModel = { title: 'Language iso code is missing' };
			return { error };
		}

		return tryExecuteAndNotify(this.#host, LanguageResource.postLanguage({ requestBody: language }));
	}

	/**
	 * Updates a Language on the server
	 * @param {LanguageModel} language
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async update(language: LanguageModel) {
		if (!language.isoCode) {
			const error: ProblemDetailsModel = { title: 'Language iso code is missing' };
			return { error };
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
		if (!isoCode) {
			const error: ProblemDetailsModel = { title: 'Iso code is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			tryExecuteAndNotify(this.#host, LanguageResource.deleteLanguageByIsoCode({ isoCode })).then(() => isoCode)
		);
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
