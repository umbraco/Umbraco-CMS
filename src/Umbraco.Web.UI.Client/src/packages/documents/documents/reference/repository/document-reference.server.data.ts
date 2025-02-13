import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDataMapperResolver } from '@umbraco-cms/backoffice/repository';

/**
 * @class UmbDocumentReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentReferenceServerDataSource extends UmbControllerBase {
	#dataMapperResolver = new UmbDataMapperResolver(this);

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @returns {*}
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferencedBy(unique: string, skip = 0, take = 20) {
		const { data, error } = await tryExecuteAndNotify(
			this,
			DocumentService.getDocumentByIdReferencedBy({ id: unique, skip, take }),
		);

		if (data) {
			const promises = data.items.map(async (item) => {
				const mapper = await this.#dataMapperResolver.resolve(item.$type);
				return mapper
					? mapper.map(item)
					: {
							...item,
							unique: item.id,
							entityType: 'unknown',
						};
			});

			const items = await Promise.all(promises);

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}
}
