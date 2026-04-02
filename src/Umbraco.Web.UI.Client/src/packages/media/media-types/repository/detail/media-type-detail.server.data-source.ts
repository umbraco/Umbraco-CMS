import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbManagementApiMediaTypeDetailDataRequestManager } from './server-data-source/media-type-detail.server.request-manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateMediaTypeRequestModel,
	MediaTypeResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the Media Type that fetches data from the server
 * @class UmbMediaTypeDetailServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeDetailServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbMediaTypeDetailModel>
{
	#detailRequestManager = new UmbManagementApiMediaTypeDetailDataRequestManager(this);

	/**
	 * Creates a new Media Type scaffold
	 * @param {Partial<UmbMediaTypeDetailModel>} [preset]
	 * @returns { CreateMediaTypeRequestModel }
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async createScaffold(preset: Partial<UmbMediaTypeDetailModel> = {}) {
		const data: UmbMediaTypeDetailModel = {
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			alias: '',
			description: '',
			icon: 'icon-picture',
			allowedAtRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			properties: [],
			containers: [],
			allowedContentTypes: [],
			compositions: [],
			collection: null,
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Media Type with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#detailRequestManager.read(unique);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Fetches multiple Media Types by their unique IDs from the server
	 * @param {Array<string>} uniques - The unique IDs of the media types to fetch
	 * @returns {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async readMany(uniques: Array<string>) {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const { data, error } = await this.#detailRequestManager.readMany(uniques);

		return {
			data: data?.items?.map((item) => this.#mapServerResponseModelToEntityDetailModel(item)),
			error,
		};
	}

	/**
	 * Inserts a new Media Type on the server
	 * @param {UmbMediaTypeDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async create(model: UmbMediaTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Media Type is missing');
		if (!model.unique) throw new Error('Media Type unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateMediaTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers,
			allowedMediaTypes: model.allowedContentTypes.map((allowedContentType) => {
				return {
					mediaType: { id: allowedContentType.contentType.unique },
					sortOrder: allowedContentType.sortOrder,
				};
			}),
			compositions: model.compositions.map((composition) => {
				return {
					mediaType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			collection: model.collection?.unique ? { id: model.collection?.unique } : null,
		};

		const { data, error } = await this.#detailRequestManager.create(body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Updates a MediaType on the server
	 * @param {UmbMediaTypeDetailModel} MediaType
	 * @param model
	 * @returns {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async update(model: UmbMediaTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateMediaTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers,
			allowedMediaTypes: model.allowedContentTypes.map((allowedContentType) => {
				return {
					mediaType: { id: allowedContentType.contentType.unique },
					sortOrder: allowedContentType.sortOrder,
				};
			}),
			compositions: model.compositions.map((composition) => {
				return {
					mediaType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
			collection: model.collection?.unique ? { id: model.collection?.unique } : null,
		};

		const { data, error } = await this.#detailRequestManager.update(model.unique, body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Deletes a Media Type on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#detailRequestManager.delete(unique);
	}

	#mapServerResponseModelToEntityDetailModel(data: MediaTypeResponseModel): UmbMediaTypeDetailModel {
		return {
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			alias: data.alias,
			description: data.description ?? '',
			icon: data.icon,
			allowedAtRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			properties: data.properties.map((property) => {
				return {
					id: property.id,
					unique: property.id,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { unique: property.dataType.id },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: data.containers as UmbPropertyTypeContainerModel[],
			allowedContentTypes: data.allowedMediaTypes.map((allowedMediaType) => {
				return {
					contentType: { unique: allowedMediaType.mediaType.id },
					sortOrder: allowedMediaType.sortOrder,
				};
			}),
			compositions: data.compositions.map((composition) => {
				return {
					contentType: { unique: composition.mediaType.id },
					compositionType: composition.compositionType,
				};
			}),
			collection: data.collection ? { unique: data.collection.id } : null,
		};
	}
}
