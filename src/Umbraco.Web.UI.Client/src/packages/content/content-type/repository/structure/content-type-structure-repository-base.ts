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
	protected _dataSource: UmbContentTypeStructureDataSource<ItemType>;

	constructor(host: UmbControllerHost, structureSource: UmbContentTypeStructureDataSourceConstructor<ItemType>) {
		super(host);
		this._dataSource = new structureSource(host);
	}

	/**
	 * Returns a promise with the allowed children of a content type
	 * @param {string} unique
	 * @param parentContentUnique
	 * @returns {*}
	 * @memberof UmbContentTypeStructureRepositoryBase
	 */
	requestAllowedChildrenOf(unique: string | null, parentContentUnique: string | null) {
		return this._dataSource.getAllowedChildrenOf(unique, parentContentUnique);
	}
}
