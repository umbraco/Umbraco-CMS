import {
	type UmbContentTypeStructureDataSource,
	UmbContentTypeStructureServerDataSourceBase,
	type UmbContentTypeStructureServerDataSourceBaseArgs,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

// Temp type
type AllowedContentTypeModel = {
	id: string;
	name: string;
	description?: string | null;
	icon?: string | null;
};

export interface UmbContentTypeUploadableStructureServerDataSourceBaseArgs<
	ServerItemType extends AllowedContentTypeModel,
	ClientItemType extends { unique: string },
> extends UmbContentTypeStructureServerDataSourceBaseArgs<ServerItemType, ClientItemType> {
	getAllowedMediaTypesOf: (fileExtension: string | null) => Promise<ServerItemType>;
}

export abstract class UmbContentTypeUploadableStructureServerDataSourceBase<
		ServerItemType extends AllowedContentTypeModel,
		ClientItemType extends { unique: string },
	>
	extends UmbContentTypeStructureServerDataSourceBase<ServerItemType, ClientItemType>
	implements UmbContentTypeStructureDataSource<ClientItemType>
{
	#host;
	#getAllowedChildrenOf;
	#mapper;

	/**
	 * Creates an instance of UmbContentTypeStructureServerDataSourceBase.
	 * @param {UmbControllerHost} host
	 * @memberof UmbItemServerDataSourceBase
	 */
	constructor(
		host: UmbControllerHost,
		args: UmbContentTypeUploadableStructureServerDataSourceBaseArgs<ServerItemType, ClientItemType>,
	) {
		super(host, args);
		this.#host = host;
		this.#getAllowedChildrenOf = args.getAllowedChildrenOf;
		this.#mapper = args.mapper;
	}

	/**
	 * Returns a promise with the allowed content types for the given unique
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbContentTypeStructureServerDataSourceBase
	 */
	async getAllowedMediaTypesOf(fileExtension: string | null) {
		const { data, error } = await tryExecuteAndNotify(this.#host, this.#getAllowedChildrenOf(fileExtension));

		if (data) {
			const items = data.items.map((item) => this.#mapper(item));
			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
