import {
	CreateMediaTypeRequestModel,
	MediaTypeResource,
	MediaTypeResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the Media Type detail that fetches data from the server
 * @export
 * @class UmbMediaTypeDetailServerDataSource
 * @implements {MediaTypeDetailDataSource}
 */
export class UmbMediaTypeDetailServerDataSource
	implements UmbDataSource<CreateMediaTypeRequestModel, any, UpdateMediaTypeRequestModel, MediaTypeResponseModel>
{
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new MediaType scaffold
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async createScaffold() {
		const data: CreateMediaTypeRequestModel = {
			name: '',
		} as CreateMediaTypeRequestModel;

		return { data };
	}

	/**
	 * @description - Fetches a MediaType with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async get(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeById({
				id: id,
			}),
		);
	}

	/**
	 * @description - Updates a MediaType on the server
	 * @param {UpdateMediaTypeRequestModel} MediaType
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async update(id: string, data: UpdateMediaTypeRequestModel) {
		if (!id) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.putMediaTypeById({
				id: id,
				requestBody: data,
			}),
		);
	}

	/**
	 * @description - Inserts a new MediaType on the server
	 * @param {CreateMediaTypeRequestModel} data
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async insert(mediaType: CreateMediaTypeRequestModel) {
		if (!mediaType) throw new Error('Media type is missing');
		if (!mediaType.id) throw new Error('Media type id is missing');

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.postMediaType({
				requestBody: mediaType,
			}),
		);
	}

	/**
	 * @description - Deletes a MediaType on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.deleteMediaTypeById({
				id: id,
			}),
		);
	}
}
