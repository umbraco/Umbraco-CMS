import { UmbDictionaryDetailRepository } from '../detail/index.js';
import type { UmbDictionaryDetailModel } from '../../types.js';
import { UmbDictionaryImportServerDataSource } from './dictionary-import.server.data-source.js';
import {
	UmbRepositoryBase,
	type UmbDataSourceResponse,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryImportRepository extends UmbRepositoryBase {
	#importSource: UmbDictionaryImportServerDataSource;
	#detailRepository: UmbDictionaryDetailRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#importSource = new UmbDictionaryImportServerDataSource(host);
		this.#detailRepository = new UmbDictionaryDetailRepository(host);
	}

	/**
	 * @description - Import a dictionary
	 * @param {string} temporaryFileUnique - The unique identifier of the uploaded temporary file to import.
	 * @param {string} [parentUnique] - The unique identifier of the parent to import into, if any.
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<UmbDictionaryDetailModel | undefined> | UmbDataSourceResponse<unknown>>} The imported dictionary.
	 * @memberof UmbDictionaryImportRepository
	 */
	async requestImport(
		temporaryFileUnique: string,
		parentUnique: string | null,
	): Promise<
		UmbRepositoryResponseWithAsObservable<UmbDictionaryDetailModel | undefined> | UmbDataSourceResponse<unknown>
	> {
		if (!temporaryFileUnique) throw new Error('Temporary file unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		const { data, error } = await this.#importSource.import(temporaryFileUnique, parentUnique);

		if (data && typeof data === 'string') {
			// Request the detail for the imported dictionary. This will also append it to the detail store
			return this.#detailRepository.requestByUnique(data);
		}

		return { data, error };
	}
}

export { UmbDictionaryImportRepository as api };
