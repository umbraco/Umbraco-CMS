import { UmbMediaTypeDetailModel } from '../../types.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import {
	CreateMediaTypeRequestModel,
	MediaTypeResource,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Media Type that fetches data from the server
 * @export
 * @class UmbMediaTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeServerDataSource implements UmbDetailDataSource<UmbMediaTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Media Type scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateMediaTypeRequestModel }
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbMediaTypeDetailModel = {
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			parentUnique,
			name: '',
			alias: '',
			description: '',
			icon: '',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			containerId: null,
			properties: [],
			containers: [],
			allowedContentTypes: [],
			compositions: [],
		};

		return { data };
	}

	/**
	 * Fetches a Media Type with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, MediaTypeResource.getMediaTypeById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const mediaType: UmbMediaTypeDetailModel = {
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			unique: data.id,
			parentUnique: data.containerId,
			name: data.name,
			alias: data.alias,
			description: data.description || null,
			icon: data.icon,
			allowedAsRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			containerId: data.containerId,
			properties: data.properties,
			containers: data.containers,
			allowedContentTypes: data.allowedContentTypes,
			compositions: data.compositions,
		};

		return { data: mediaType };
	}

	/**
	 * Inserts a new Media Type on the server
	 * @param {UmbMediaTypeDetailModel} mediaType
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async create(mediaType: UmbMediaTypeDetailModel) {
		if (!mediaType) throw new Error('Media Type is missing');
		if (!mediaType.unique) throw new Error('Media Type unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMediaTypeRequestModel = {
			alias: mediaType.alias,
			name: mediaType.name,
			description: mediaType.description,
			icon: mediaType.icon,
			allowedAsRoot: mediaType.allowedAsRoot,
			variesByCulture: mediaType.variesByCulture,
			variesBySegment: mediaType.variesBySegment,
			isElement: mediaType.isElement,
			properties: mediaType.properties,
			containers: mediaType.containers,
			allowedContentTypes: mediaType.allowedContentTypes,
			compositions: mediaType.compositions,
			id: mediaType.unique,
			containerId: mediaType.parentUnique,
		};

		const { error: createError } = await tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.postMediaType({
				requestBody,
			}),
		);

		if (createError) {
			return { error: createError };
		}

		// We have to fetch the data type again. The server can have modified the data after creation
		return this.read(mediaType.unique);
	}

	/**
	 * Updates a MediaType on the server
	 * @param {UmbMediaTypeDetailModel} MediaType
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async update(data: UmbMediaTypeDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateMediaTypeRequestModel = {
			alias: data.alias,
			name: data.name,
			description: data.description,
			icon: data.icon,
			allowedAsRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			properties: data.properties,
			containers: data.containers,
			allowedContentTypes: data.allowedContentTypes,
			compositions: data.compositions,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.putMediaTypeById({
				id: data.unique,
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// We have to fetch the data type again. The server can have modified the data after update
		return this.read(data.unique);
	}

	/**
	 * Deletes a Media Type on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.deleteMediaTypeById({
				id: unique,
			}),
		);
	}
}
