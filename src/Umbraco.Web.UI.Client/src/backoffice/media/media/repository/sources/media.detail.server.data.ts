import type { MediaDetails } from '../../';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	CreateMediaRequestModel,
	ProblemDetailsModel,
	UpdateMediaRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbMediaDetailServerDataSource
	implements UmbDataSource<CreateMediaRequestModel, UpdateMediaRequestModel, MediaDetails>
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
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
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
			}) as any
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

		let body: string;

		try {
			body = JSON.stringify(media);
		} catch (error) {
			const myError: ProblemDetailsModel = { title: 'JSON could not parse' };
			return { error: myError };
		}

		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/save', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
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
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
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
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		let problemDetails: ProblemDetailsModel | undefined = undefined;

		try {
			await fetch('/umbraco/management/api/v1/media/delete', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			});
		} catch (error) {
			problemDetails = { title: 'Delete document Failed' };
		}

		return { error: problemDetails };

		/* TODO: use backend cli when available.
		return tryExecuteAndNotify(this.#host);
		*/
	}
}
