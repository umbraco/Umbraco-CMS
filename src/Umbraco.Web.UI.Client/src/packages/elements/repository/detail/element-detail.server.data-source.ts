import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { umbMapElementCreateRequestBody, umbMapElementUpdateRequestBody } from './element-detail-request.mappers.js';
import { umbMapElementResponseToDetailModel } from './element-detail-response.mappers.js';
import { UmbManagementApiElementDetailDataRequestManager } from './element-detail.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Element that fetches data from the server
 * @class UmbElementServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbElementServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbElementDetailModel>
{
	#detailRequestManager = new UmbManagementApiElementDetailDataRequestManager(this);

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
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#detailRequestManager.read(unique);

		return { data: data ? umbMapElementResponseToDetailModel(data) : undefined, error };
	}

	/**
	 * Fetches multiple Elements by their unique IDs from the server
	 * @param {Array<string>} uniques - The unique IDs of the elements to fetch
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async readMany(uniques: Array<string>) {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const { data, error } = await this.#detailRequestManager.readMany(uniques);

		return {
			data: data?.items?.map((item) => umbMapElementResponseToDetailModel(item)),
			error,
		};
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

		const { data, error } = await this.#detailRequestManager.create(body);

		return { data: data ? umbMapElementResponseToDetailModel(data) : undefined, error };
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

		const { data, error } = await this.#detailRequestManager.update(model.unique, body);

		return { data: data ? umbMapElementResponseToDetailModel(data) : undefined, error };
	}

	/**
	 * Deletes an Element on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return this.#detailRequestManager.delete(unique);
	}
}
