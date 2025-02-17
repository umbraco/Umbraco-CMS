import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDataMapper } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @class UmbMediaReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaReferenceServerDataSource extends UmbControllerBase {
	#dataMapper = new UmbDataMapper(this);

	/**
	 * Fetches the item for the given id from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @returns {*}
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	async getReferencedBy(unique: string, skip = 0, take = 20) {
		const { data, error } = await tryExecuteAndNotify(
			this,
			MediaService.getMediaByIdReferencedBy({ id: unique, skip, take }),
		);

		if (data) {
			const promises = data.items.map(async (item) => {
				return this.#dataMapper.map({
					dataSourceIdentifier: 'Umb.ManagementApi',
					dataModelIdentifier: item.$type,
					data: item,
					fallback: async () => {
						return {
							...item,
							unique: item.id,
							entityType: 'unknown',
						};
					},
				});
			});

			const items = await Promise.all(promises);

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}
}
