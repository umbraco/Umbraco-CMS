import type { MediaDetails } from '../../index.js';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { CreateMediaRequestModel, UpdateMediaRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbMediaDetailServerDataSource
	implements UmbDataSource<CreateMediaRequestModel, any, UpdateMediaRequestModel, MediaDetails>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbMediaDetailServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaDetailServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Media with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			// TODO: use backend cli when available.
			fetch(`/umbraco/management/api/v1/media/details/${id}`)
				.then((res) => res.json())
				.then((res) => res[0] || undefined)
		);
	}

	/**
	 * Creates a new Media scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const data = {
			id: '',
			name: '',
			icon: '',
			parentId,
			contentTypeId: '',
			properties: [],
			data: [],
			variants: [],
		};

		return { data };
	}

	/**
	 * Inserts a new Media on the server
	 * @param {Media} media
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async insert(media: CreateMediaRequestModel) {
		if (!media) throw new Error('Media is missing');

		let body: string;

		try {
			body = JSON.stringify(media);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}
		//return tryExecuteAndNotify(this.#host, MediaResource.postMedia(payload));
		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/save', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}

	/**
	 * Updates a Media on the server
	 * @param {Media} Media
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(id: string, media: UpdateMediaRequestModel) {
		if (!id) throw new Error('Key is missing');
		if (!media) throw new Error('Media is missing');

		const body = JSON.stringify(media);

		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/save', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}

	/**
	 * Trash a Media on the server
	 * @param {Media} Media
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async trash(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Key is missing');
		}

		return tryExecuteAndNotify<undefined>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/delete', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}
}
