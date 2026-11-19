import type { UmbContentTypeStructureRepository } from './content-type-structure-repository.interface.js';
import type {
	UmbContentTypeStructureDataSource,
	UmbContentTypeStructureDataSourceConstructor,
} from './content-type-structure-data-source.interface.js';
import {
	fetchAllPages,
	UmbRepositoryBase,
	type UmbDataSourceResponse,
	type UmbPagedModel,
} from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

// Mirrors the server's default `take` for the allowed-children and allowed-at-root endpoints.
const ALLOWED_CHILDREN_PAGE_SIZE = 100;

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
	 * Returns a promise with a single page of the allowed children of a content type. The server returns the
	 * first 100 when no paging is given, so prefer `requestAllAllowedChildrenOf` when every allowed child is needed.
	 * @param {string | null} unique - The content type to get the allowed children of, or `null` for the root.
	 * @param {string | null} parentContentUnique - The content item the children will be created under, if any.
	 * @param {UmbOffsetPaginationRequestModel} paging - The page to return.
	 * @returns {Promise} A promise resolving to `{ data: { items, total } }`, or `{ error }`.
	 * @memberof UmbContentTypeStructureRepositoryBase
	 */
	requestAllowedChildrenOf(
		unique: string | null,
		parentContentUnique: string | null,
		paging?: UmbOffsetPaginationRequestModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>> {
		return this._dataSource.getAllowedChildrenOf(unique, parentContentUnique, paging);
	}

	/**
	 * Returns a promise with every allowed child of a content type, by paging through the data source until all
	 * items have been retrieved.
	 * @param {string | null} unique - The content type to get the allowed children of, or `null` for the root.
	 * @param {string | null} parentContentUnique - The content item the children will be created under, if any.
	 * @returns {Promise} A promise resolving to `{ data: { items, total } }` containing every allowed child, or `{ error }`.
	 * @memberof UmbContentTypeStructureRepositoryBase
	 */
	requestAllAllowedChildrenOf(unique: string | null, parentContentUnique: string | null) {
		return fetchAllPages<ItemType>(
			(skip, take) => this._dataSource.getAllowedChildrenOf(unique, parentContentUnique, { skip, take }),
			ALLOWED_CHILDREN_PAGE_SIZE,
		);
	}
}
