import { MediaTypeDetailDataSource } from './media-type.details.server.data.interface';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import type { MediaTypeDetails } from '@umbraco-cms/backoffice/models';
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
	 * @description - Fetches a MediaType with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	get(key: string) {
		//return tryExecuteAndNotify(this.#host, MediaTypeResource.getMediaTypeByKey({ key })) as any;
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/media-type/${key}`)) as any;
	}

	/**
	 * @description - Updates a MediaType on the server
	 * @param {MediaTypeDetails} MediaType
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async update(mediaType: MediaTypeDetails) {
		if (!mediaType.key) {
			const error: ProblemDetailsModel = { title: 'MediaType key is missing' };
			return { error };
		}

		const payload = { key: mediaType.key, requestBody: mediaType };
		//return tryExecuteAndNotify(this.#host, MediaTypeResource.putMediaTypeByKey(payload));

		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/${mediaType.key}`, {
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
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMediaTypeDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		//return await tryExecuteAndNotify(this.#host, MediaTypeResource.deleteMediaTypeByKey({ key }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/media-type/${key}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
