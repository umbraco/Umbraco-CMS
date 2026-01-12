//import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
//import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { /*tryExecute,*/ UmbError } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
//import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityReferenceDataSource, UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @class UmbElementReferenceServerDataSource
 * @implements {UmbEntityReferenceDataSource}
 */
export class UmbElementReferenceServerDataSource extends UmbControllerBase implements UmbEntityReferenceDataSource {
	//#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>>} - Items that are referenced by the given unique
	 * @memberof UmbElementReferenceServerDataSource
	 */
	async getReferencedBy(
		unique: string,
		skip = 0,
		take = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>> {
		return { error: new UmbError(`'getReferencedBy' has not been implemented yet. (${unique}, ${skip}, ${take})`) };

		// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
		// const { data, error } = await tryExecute(
		// 	this,
		// 	ElementService.getElementByIdReferencedBy({ path: { id: unique }, query: { skip, take } }),
		// );

		// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
		// if (data) {
		// 	const promises = data.items.map(async (item) => {
		// 		return this.#dataMapper.map({
		// 			forDataModel: item.$type,
		// 			data: item,
		// 			fallback: async () => {
		// 				return {
		// 					...item,
		// 					unique: item.id,
		// 					entityType: 'unknown',
		// 				};
		// 			},
		// 		});
		// 	});

		// 	const items = await Promise.all(promises);

		// 	return { data: { items, total: data.total } };
		// }

		//return { data, error };
	}

	/**
	 * Checks if the items are referenced by other items
	 * @param {Array<string>} uniques - The unique identifiers of the items to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Items that are referenced by other items
	 * @memberof UmbElementReferenceServerDataSource
	 */
	async getAreReferenced(
		uniques: Array<string>,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		return {
			error: new UmbError(`'getElementAreReferenced' has not been implemented yet. (${uniques}, ${skip}, ${take})`),
		};

		// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
		// const { data, error } = await tryExecute(
		// 	this,
		// 	ElementService.getElementAreReferenced({ query: { id: uniques, skip, take } }),
		// );

		// if (data) {
		// 	const items: Array<UmbEntityModel> = data.items.map((item) => {
		// 		return {
		// 			unique: item.id,
		// 			entityType: UMB_ELEMENT_ENTITY_TYPE,
		// 		};
		// 	});

		// 	return { data: { items, total: data.total } };
		// }

		//return { data, error };
	}

	/**
	 * Returns any descendants of the given unique that is referenced by other items
	 * @param {string} unique - The unique identifier of the item to fetch descendants for
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Any descendants of the given unique that is referenced by other items
	 * @memberof UmbElementReferenceServerDataSource
	 */
	async getReferencedDescendants(
		unique: string,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		return {
			error: new UmbError(
				`'getElementByIdReferencedDescendants' has not been implemented yet. (${unique}, ${skip}, ${take})`,
			),
		};

		// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
		// const { data, error } = await tryExecute(
		// 	this,
		// 	ElementService.getElementByIdReferencedDescendants({ path: { id: unique }, query: { skip, take } }),
		// );

		// if (data) {
		// 	const items: Array<UmbEntityModel> = data.items.map((item) => {
		// 		return {
		// 			unique: item.id,
		// 			entityType: UMB_ELEMENT_ENTITY_TYPE,
		// 		};
		// 	});

		// 	return { data: { items, total: data.total } };
		// }

		//return { data, error };
	}
}
