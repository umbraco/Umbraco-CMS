import type { MediaTypeDetails } from '../../types.js';
import { MediaTypeDetailDataSource } from './media-type.details.server.data.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * @description - A data source for the Media Type detail that fetches data from the server
 * @export
 * @class UmbMediaTypeDetailServerDataSource
 * @implements {MediaTypeDetailDataSource}
 */
export class UmbMediaTypeDetailServerDataSource implements MediaTypeDetailDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new MediaType scaffold
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async createScaffold() {
		const data: MediaTypeDetails = {
			name: '',
		} as MediaTypeDetails;

		return { data };
	}

	/**
	 * @description - Fetches a MediaType with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	get(id: string) {
		//return tryExecuteAndNotify(this.#host, MediaTypeResource.getMediaTypeByKey({ id })) as any;
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/media-type/${id}`)) as any;
	}

	/**
	 * @description - Updates a MediaType on the server
	 * @param {MediaTypeDetails} MediaType
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async update(mediaType: MediaTypeDetails) {
		if (!mediaType.id) {
			throw new Error('MediaType id is missing');
		}

		const payload = { id: mediaType.id, requestBody: mediaType };
		//return tryExecuteAndNotify(this.#host, MediaTypeResource.putMediaTypeByKey(payload));

		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/${mediaType.id}`, {
				method: 'PUT',
				body: JSON.stringify(payload),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Inserts a new MediaType on the server
	 * @param {MediaTypeDetails} data
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async insert(data: MediaTypeDetails) {
		//return tryExecuteAndNotify(this.#host, MediaTypeResource.postMediaType({ requestBody: data }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/`, {
				method: 'POST',
				body: JSON.stringify(data),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Deletes a MediaType on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		//return await tryExecuteAndNotify(this.#host, MediaTypeResource.deleteMediaTypeByKey({ id }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/${id}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
