import type {
	UmbContentTypeUploadableStructureDataSource,
	UmbContentTypeUploadableStructureDataSourceConstructor,
} from './content-type-uploadable-structure-data-source.interface.js';
import type { UmbContentTypeUploadableStructureRepository } from './content-type-uploadable-structure-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export abstract class UmbContentTypeUploadableStructureRepositoryBase<ItemUploadableType>
	extends UmbContentTypeStructureRepositoryBase<ItemUploadableType>
	implements UmbContentTypeUploadableStructureRepository<ItemUploadableType>
{
	#structureSource: UmbContentTypeUploadableStructureDataSource<ItemUploadableType>;

	constructor(
		host: UmbControllerHost,
		structureSource: UmbContentTypeUploadableStructureDataSourceConstructor<ItemUploadableType>,
	) {
		super(host, structureSource);
		this.#structureSource = new structureSource(host);
	}

	/**
	 * Returns a promise with the allowed media-types of a uploadable content type.
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbContentTypeUploadableStructureRepositoryBase
	 */
	requestAllowedMediaTypesOf(fileExtension: string | null) {
		return this.#structureSource.getAllowedMediaTypesOf(fileExtension);
	}
}
