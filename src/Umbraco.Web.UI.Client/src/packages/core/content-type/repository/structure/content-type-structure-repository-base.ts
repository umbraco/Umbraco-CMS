import type { UmbContentTypeStructureRepository } from './content-type-structure-repository.interface.js';
import type {
	UmbContentTypeStructureDataSource,
	UmbContentTypeStructureDataSourceConstructor,
} from './content-type-structure-data-source.interface.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbContentTypeStructureRepositoryBase<ItemType>
	extends UmbRepositoryBase
	implements UmbContentTypeStructureRepository<ItemType>
{
	#structureSource: UmbContentTypeStructureDataSource<ItemType>;

	constructor(host: UmbControllerHost, structureSource: UmbContentTypeStructureDataSourceConstructor<ItemType>) {
		super(host);
		this.#structureSource = new structureSource(host);
	}

	/**
	 * Returns a promise with the allowed children of a content type
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbContentTypeStructureRepositoryBase
	 */
	requestAllowedChildrenOf(unique: string | null) {
		return this.#structureSource.getAllowedChildrenOf(unique);
	}
}
