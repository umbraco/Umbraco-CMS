import type { UmbStaticFileItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StaticFileService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A server data source for Static File items
 * @class UmbStaticFileItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbStaticFileItemServerDataSource extends UmbItemServerDataSourceBase<
	StaticFileItemResponseModel,
	UmbStaticFileItemModel
> {
	/**
	 * Creates an instance of UmbStaticFileItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStaticFileItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const serializer = new UmbServerFilePathUniqueSerializer();
		const paths = uniques.map((unique) => serializer.toServerPath(unique)!);

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => StaticFileService.getItemStaticFile({ query: { path: args.uniques } }),
			uniques: paths,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: StaticFileItemResponseModel): UmbStaticFileItemModel => {
	const serializer = new UmbServerFilePathUniqueSerializer();
	return {
		isFolder: item.isFolder,
		name: item.name,
		parentUnique: item.parent ? serializer.toUnique(item.parent.path) : null,
		unique: serializer.toUnique(item.path),
	};
};
