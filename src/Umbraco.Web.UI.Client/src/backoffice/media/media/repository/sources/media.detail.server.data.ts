import type { MediaDetails } from '../../';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbMediaDetailServerDataSource implements UmbDataSource<MediaDetails> {
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
	 * Fetches a Media with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async get(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			// TODO: use backend cli when available.
			fetch(`/umbraco/management/api/v1/media/details/${key}`)
				.then((res) => res.json())
				.then((res) => res[0] || undefined)
		);
	}

	/**
	 * Creates a new Media scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
		const data: MediaDetails = {
			$type: '',
			key: '',
			name: '',
			icon: '',
			type: '',
			hasChildren: false,
			parentKey: parentKey ?? '',
			isTrashed: false,
			properties: [
				{
					alias: '',
					label: '',
					description: '',
					dataTypeKey: '',
				},
			],
			data: [
				{
					alias: '',
					value: '',
				},
			],
			variants: [
				{
					name: '',
				},
			],
		};

		return { data };
	}

	/**
	 * Inserts a new Media on the server
	 * @param {Media} media
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async insert(media: MediaDetails) {
		if (!media.key) {
			//const error: ProblemDetails = { title: 'Media key is missing' };
			return Promise.reject();
		}
		//const payload = { key: media.key, requestBody: media };

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
	async update(media: MediaDetails) {
		if (!media.key) {
			const error: ProblemDetailsModel = { title: 'Media key is missing' };
			return { error };
		}
		//const payload = { key: media.key, requestBody: media };

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
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify<MediaDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify([key]),
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
		);
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		let problemDetails: ProblemDetailsModel | undefined = undefined;

		try {
			await fetch('/umbraco/management/api/v1/media/delete', {
				method: 'POST',
				body: JSON.stringify([key]),
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
