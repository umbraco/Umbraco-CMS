import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { umbMapElementCreateRequestBody, umbMapElementUpdateRequestBody } from './element-detail-request.mappers.js';
import { umbMapElementResponseToDetailModel } from './element-detail-response.mappers.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDataSourceResponse, UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Element that fetches data from the server
 * @class UmbElementServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbElementServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbElementDetailModel>
{
	/**
	 * Creates a new Element scaffold
	 * @param preset
	 * @returns { UmbElementDetailModel }
	 * @memberof UmbElementServerDataSource
	 */
	async createScaffold(preset: Partial<UmbElementDetailModel> = {}) {
		const data: UmbElementDetailModel = {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			unique: UmbId.new(),
			documentType: {
				unique: '',
				collection: null,
			},
			isTrashed: false,
			values: [],
			variants: [],
			flags: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches an Element with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbElementDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this, ElementService.getElementById({ path: { id: unique } }));

		if (error || !data) {
			return { error };
		}

		const element = umbMapElementResponseToDetailModel(data);

		return { data: element };
	}

	/**
	 * Fetches multiple Elements by their unique IDs from the server.
	 * Elements the current user cannot access, or that do not exist, are omitted from the result.
	 * @param {Array<string>} uniques - The unique IDs of the elements to fetch
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async readMany(uniques: Array<string>) {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const getItemsController = new UmbItemDataApiGetRequestController(this, {
			api: async (args) => ({
				// eslint-disable-next-line local-rules/no-direct-api-import
				data: (await ElementService.getElementBatch({ query: { id: args.uniques } })).data.items,
			}),
			uniques,
		});

		const { data, error } = await getItemsController.request();

		return { data: data?.map((item) => umbMapElementResponseToDetailModel(item)), error };
	}

	/**
	 * Inserts a new element on the server
	 * @param {UmbElementDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async create(model: UmbElementDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Element is missing');
		if (!model.unique) throw new Error('Element unique is missing');

		const body = umbMapElementCreateRequestBody(model, parentUnique);

		const { data, error } = await tryExecute(
			this,
			ElementService.postElement({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates an Element on the server
	 * @param {UmbElementDetailModel} model
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async update(model: UmbElementDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const body = umbMapElementUpdateRequestBody(model);

		const { error } = await tryExecute(
			this,
			ElementService.putElementById({
				path: { id: model.unique },
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes an Element on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this, ElementService.deleteRecycleBinElementById({ path: { id: unique } }));
	}
}
