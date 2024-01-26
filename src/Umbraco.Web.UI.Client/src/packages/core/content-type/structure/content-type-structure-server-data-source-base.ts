import type { UmbPagedModel } from '../../repository/data-source/types.js';
import type { UmbContentTypeStructureDataSource } from './content-type-structure-data-source.interface.js';
import type {
	AllowedContentTypeModel,
	CancelablePromise,
	ItemResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export interface UmbContentTypeStructureServerDataSourceBaseArgs<
	ServerItemType extends ItemResponseModelBaseModel,
	ClientItemType extends { unique: string },
> {
	getAllowedAtRoot: () => CancelablePromise<UmbPagedModel<ServerItemType>>;
	getAllowedChildrenOf: (unique: string) => CancelablePromise<UmbPagedModel<ServerItemType>>;
	mapper: (item: ServerItemType) => ClientItemType;
}

export abstract class UmbContentTypeStructureServerDataSourceBase<
	ServerItemType extends AllowedContentTypeModel,
	ClientItemType extends { unique: string },
> implements UmbContentTypeStructureDataSource<ClientItemType>
{
	#host;
	#getAllowedAtRoot;
	#getAllowedChildrenOf;
	#mapper;

	/**
	 * Creates an instance of UmbContentTypeStructureServerDataSourceBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbItemServerDataSourceBase
	 */
	constructor(
		host: UmbControllerHost,
		args: UmbContentTypeStructureServerDataSourceBaseArgs<ServerItemType, ClientItemType>,
	) {
		this.#host = host;
		this.#getAllowedAtRoot = args.getAllowedAtRoot;
		this.#getAllowedChildrenOf = args.getAllowedChildrenOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Returns a promise with the allowed content types at root
	 * @return {*}
	 * @memberof UmbContentTypeStructureServerDataSourceBase
	 */
	async allowedAtRoot() {
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getAllowedAtRoot());

		if (data) {
			const items = data.items.map((item) => this.#mapper(item));
			return { data: { items, total: data.total } };
		}

		return { error };
	}

	/**
	 * Returns a promise with the allowed content types for the given unique
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbContentTypeStructureServerDataSourceBase
	 */
	async allowedChildrenOf(unique: string) {
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getAllowedChildrenOf(unique));

		if (data) {
			const items = data.items.map((item) => this.#mapper(item));
			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
