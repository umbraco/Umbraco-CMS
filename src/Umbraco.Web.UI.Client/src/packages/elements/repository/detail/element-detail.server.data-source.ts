import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDataSourceResponse, UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateElementRequestModel,
	ElementResponseModel,
	UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Element that fetches data from the server
 * @class UmbElementServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbElementServerDataSource implements UmbDetailDataSource<UmbElementDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

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
	 * Creates a new variant scaffold.
	 * @returns A new variant scaffold.
	 */
	/*
	// TODO: remove if not used
	createVariantScaffold(): UmbElementVariantModel {
		return {
			state: null,
			culture: null,
			segment: null,
			name: '',
			publishDate: null,
			createDate: null,
			updateDate: null,
		};
	}
	*/

	/**
	 * Fetches a Element with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbElementDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, ElementService.getElementById({ path: { id: unique } }));

		if (error || !data) {
			return { error };
		}

		const element = this.#createElementDetailModel(data);

		return { data: element };
	}

	/**
	 * Inserts a new Element on the server
	 * @param {UmbElementDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async create(model: UmbElementDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Element is missing');
		if (!model.unique) throw new Error('Element unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateElementRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecute(
			this.#host,
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
	 * Updates a Element on the server
	 * @param {UmbElementDetailModel} Element
	 * @param model
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async update(model: UmbElementDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateElementRequestModel = {
			values: model.values,
			variants: model.variants,
		};

		const { error } = await tryExecute(
			this.#host,
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
	 * Deletes a Element on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbElementServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, ElementService.deleteElementById({ path: { id: unique } }));
	}

	#createElementDetailModel(data: ElementResponseModel): UmbElementDetailModel {
		return {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			unique: data.id,
			values: data.values.map((value) => {
				return {
					editorAlias: value.editorAlias,
					culture: value.culture || null,
					segment: value.segment || null,
					alias: value.alias,
					value: value.value,
				};
			}),
			variants: data.variants.map((variant) => {
				return {
					culture: variant.culture || null,
					segment: variant.segment || null,
					state: variant.state,
					name: variant.name,
					publishDate: variant.publishDate || null,
					createDate: variant.createDate,
					updateDate: variant.updateDate,
					scheduledPublishDate: variant.scheduledPublishDate || null,
					scheduledUnpublishDate: variant.scheduledUnpublishDate || null,
					flags: [], //variant.flags,
				};
			}),
			documentType: {
				unique: data.documentType.id,
				collection: null,
			},
			isTrashed: data.isTrashed,
			flags: data.flags,
		};
	}
}
