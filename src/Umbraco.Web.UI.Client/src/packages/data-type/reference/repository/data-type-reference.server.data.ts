import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityReferenceDataSource, UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';

/**
 * @class UmbDataTypeReferenceServerDataSource
 * @implements {UmbEntityReferenceDataSource}
 */
export class UmbDataTypeReferenceServerDataSource extends UmbControllerBase implements UmbEntityReferenceDataSource {
	#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>>} - Items that are referenced by the given unique
	 * @memberof UmbDataTypeReferenceServerDataSource
	 */
	async getReferencedBy(
		unique: string,
		skip = 0,
		take = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>> {
		const { data, error } = await tryExecute(
			this,
			DataTypeService.getDataTypeByIdReferencedBy({ id: unique, skip, take }),
		);

		if (data) {
			const promises = data.items.map(async (item) => {
				return this.#dataMapper.map({
					forDataModel: item.$type,
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

	/**
	 * Checks if the items are referenced by other items
	 * @param {Array<string>} uniques - The unique identifiers of the items to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Items that are referenced by other items
	 * @memberof UmbDataTypeReferenceServerDataSource
	 */
	async getAreReferenced(
		uniques: Array<string>,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		skip: number = 0,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		console.warn('getAreReferenced is not implemented for DataTypeReferenceServerDataSource');
		return { data: { items: [], total: 0 } };
	}
}
