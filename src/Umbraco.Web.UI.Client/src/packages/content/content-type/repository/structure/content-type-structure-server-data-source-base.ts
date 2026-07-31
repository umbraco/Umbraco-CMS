import type { UmbContentTypeStructureDataSource } from './content-type-structure-data-source.interface.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

// Keep this type internal
type AllowedContentTypeBaseModel = {
	id: string;
	name: string;
	description?: string | null;
	icon?: string | null;
};

export interface UmbContentTypeStructureServerDataSourceBaseArgs<
	ServerItemType extends AllowedContentTypeBaseModel,
	ClientItemType extends UmbEntityModel,
> {
	getAllowedChildrenOf: (
		unique: string | null,
		parentContentUnique: string | null,
		paging?: UmbOffsetPaginationRequestModel,
	) => Promise<UmbDataSourceResponse<UmbPagedModel<ServerItemType>>>;
	mapper: (item: ServerItemType) => ClientItemType;
}

export abstract class UmbContentTypeStructureServerDataSourceBase<
	ServerItemType extends AllowedContentTypeBaseModel,
	ClientItemType extends UmbEntityModel,
> implements UmbContentTypeStructureDataSource<ClientItemType>
{
	#host;
	#getAllowedChildrenOf;
	#mapper;

	/**
	 * Creates an instance of UmbContentTypeStructureServerDataSourceBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param args
	 * @memberof UmbItemServerDataSourceBase
	 */
	constructor(
		host: UmbControllerHost,
		args: UmbContentTypeStructureServerDataSourceBaseArgs<ServerItemType, ClientItemType>,
	) {
		this.#host = host;
		this.#getAllowedChildrenOf = args.getAllowedChildrenOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Returns a promise with a single page of the allowed content types for the given unique
	 * @param {string | null} unique - The content type to get the allowed children of, or `null` for the root.
	 * @param {string | null} parentContentUnique - The content item the children will be created under, if any.
	 * @param {UmbOffsetPaginationRequestModel} paging - The page to return.
	 * @returns {*}
	 * @memberof UmbContentTypeStructureServerDataSourceBase
	 */
	async getAllowedChildrenOf(
		unique: string | null,
		parentContentUnique: string | null,
		paging?: UmbOffsetPaginationRequestModel,
	) {
		const { data, error } = await tryExecute(
			this.#host,
			this.#getAllowedChildrenOf(unique, parentContentUnique, paging),
		);

		if (data) {
			const items = data.items.map((item) => this.#mapper(item));
			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
