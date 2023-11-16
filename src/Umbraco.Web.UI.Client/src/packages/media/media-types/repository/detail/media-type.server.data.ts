import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	CreateMediaTypeRequestModel,
	MediaTypeResource,
	MediaTypeResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';

/**
 * A data source for the Media Type that fetches data from the server
 * @export
 * @class UmbMediaTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeServerDataSource
	implements UmbDataSource<CreateMediaTypeRequestModel, any, UpdateMediaTypeRequestModel, MediaTypeResponseModel>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Media with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeById({
				id: id,
			}),
		);
	}

	/**
	 * Creates a new Media scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		//, parentId: string | null
		const data: MediaTypeResponseModel = {
			id: UmbId.new(),
			//parentId: parentId,
			name: '',
			alias: '',
			description: '',
			icon: 'icon-media',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			allowedContentTypes: [],
			compositions: [],
			properties: [],
			containers: [],
		};

		return { data };
	}

	/**
	 * Inserts a new Media Type on the server
	 * @param {CreateMediaTypeRequestModel} mediaType
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async insert(mediaType: CreateMediaTypeRequestModel) {
		if (!mediaType) throw new Error('Media Type is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.postMediaType({
				requestBody: mediaType,
			}),
		);
	}

	/**
	 * Updates a Media Type on the server
	 * @param {string} id
	 * @param {Media} mediaType
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async update(id: string, mediaType: UpdateMediaTypeRequestModel) {
		if (!id) throw new Error('Id is missing');

		mediaType = { ...mediaType };

		// TODO: Hack to remove some props that ruins the media-type post end-point.
		(mediaType as any).id = undefined;

		return tryExecuteAndNotify(this.#host, MediaTypeResource.putMediaTypeById({ id, requestBody: mediaType }));
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		// TODO: Hack the type to avoid type-error here:
		return tryExecuteAndNotify(this.#host, MediaTypeResource.deleteMediaTypeById({ id })) as any;
	}

	/**
	 * Get the allowed media types for a given parent id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeServerDataSource
	 */
	async getAllowedChildrenOf(id: string) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/allowed-children-of/${id}`, {
				method: 'GET',
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json()),
		);
	}
}
