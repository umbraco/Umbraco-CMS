import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityReferenceDataSource, UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @class UmbMediaReferenceServerDataSource
 * @implements {UmbEntityReferenceDataSource}
 */
export class UmbMediaReferenceServerDataSource extends UmbControllerBase implements UmbEntityReferenceDataSource {
	#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>>} - Items that are referenced by the given unique
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	async getReferencedBy(
		unique: string,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>> {
		const { data, error } = await tryExecuteAndNotify(
			this,
			MediaService.getMediaByIdReferencedBy({ id: unique, skip, take }),
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
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	async getAreReferenced(
		uniques: Array<string>,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		const { data, error } = await tryExecuteAndNotify(
			this,
			MediaService.getMediaAreReferenced({ id: uniques, skip, take }),
		);

		if (data) {
			const items: Array<UmbEntityModel> = data.items.map((item) => {
				return {
					unique: item.id,
					entityType: UMB_MEDIA_ENTITY_TYPE,
				};
			});

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}

	/**
	 * Returns any descendants of the given unique that is referenced by other items
	 * @param {string} unique - The unique identifier of the item to fetch descendants for
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Any descendants of the given unique that is referenced by other items
	 * @memberof UmbMediaReferenceServerDataSource
	 */
	async getReferencedDescendants(
		unique: string,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		const { data, error } = await tryExecuteAndNotify(
			this,
			MediaService.getMediaByIdReferencedDescendants({ id: unique, skip, take }),
		);

		if (data) {
			const items: Array<UmbEntityModel> = data.items.map((item) => {
				return {
					unique: item.id,
					entityType: UMB_MEDIA_ENTITY_TYPE,
				};
			});

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}
}
