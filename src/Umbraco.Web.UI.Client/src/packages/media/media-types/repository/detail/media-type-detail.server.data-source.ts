import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateMediaTypeRequestModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbPropertyContainerTypes } from '@umbraco-cms/backoffice/content-type';

/**
 * A data source for the Media Type that fetches data from the server
 * @class UmbMediaTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeServerDataSource implements UmbDetailDataSource<UmbMediaTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Media Type scaffold
	 * @param {Partial<UmbMediaTypeDetailModel>} [preset]
	 * @returns { CreateMediaTypeRequestModel }
	 * @memberof UmbMediaTypeServerDataSource
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
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, MediaTypeService.getMediaTypeById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const mediaType: UmbMediaTypeDetailModel = {
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
			containers: data.containers.map((container) => {
				return {
					id: container.id,
					parent: container.parent ? { id: container.parent.id } : null,
					name: container.name ?? '',
					type: container.type as UmbPropertyContainerTypes, // TODO: check if the value is valid
					sortOrder: container.sortOrder,
				};
			}),
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

		return { data: mediaType };
	}

	/**
	 * Inserts a new Media Type on the server
	 * @param {UmbMediaTypeDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async create(model: UmbMediaTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Media Type is missing');
		if (!model.unique) throw new Error('Media Type unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMediaTypeRequestModel = {
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
					id: property.id,
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

		const { data, error } = await tryExecute(
			this.#host,
			MediaTypeService.postMediaType({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a MediaType on the server
	 * @param {UmbMediaTypeDetailModel} MediaType
	 * @param model
	 * @returns {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async update(model: UmbMediaTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateMediaTypeRequestModel = {
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
					id: property.id,
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

		const { error } = await tryExecute(
			this.#host,
			MediaTypeService.putMediaTypeById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Media Type on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			MediaTypeService.deleteMediaTypeById({
				id: unique,
			}),
		);
	}
}
