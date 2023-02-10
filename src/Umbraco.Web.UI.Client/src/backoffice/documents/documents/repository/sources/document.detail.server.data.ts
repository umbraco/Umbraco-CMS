import { RepositoryDetailDataSource } from '@umbraco-cms/repository';
import { ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import type { DataSourceResponse, DocumentDetails } from '@umbraco-cms/models';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbDocumentDetailServerDataSource implements RepositoryDetailDataSource<DocumentDetails> {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbDocumentDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches a Document with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	async get(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			// TODO: use backend cli when available.
			fetch(`/umbraco/management/api/v1/document/details/${key}`)
				.then((res) => res.json())
				.then((res) => res[0] || undefined)
		);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
		const data: DocumentDetails = {
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
		} as DocumentDetails;

		return { data };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {Document} document
	 * @return {*}
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	async insert(document: DocumentDetails) {
		if (!document.key) {
			//const error: ProblemDetails = { title: 'Document key is missing' };
			return Promise.reject();
		}
		//const payload = { key: document.key, requestBody: document };

		let body: string;

		try {
			body = JSON.stringify(document);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}
		//return tryExecuteAndNotify(this.#host, DocumentResource.postDocument(payload));
		return tryExecuteAndNotify<DocumentDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/document/save', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
		);
	}

	/**
	 * Updates a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(document: DocumentDetails) {
		if (!document.key) {
			const error: ProblemDetailsModel = { title: 'Document key is missing' };
			return { error };
		}
		//const payload = { key: document.key, requestBody: document };

		let body: string;

		try {
			body = JSON.stringify(document);
		} catch (error) {
			const myError: ProblemDetailsModel = { title: 'JSON could not parse' };
			return { error: myError };
		}

		return tryExecuteAndNotify<DocumentDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/document/save', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
		);
	}

	/**
	 * Trash a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbDocumentDetailServerDataSource
	 */
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify<DocumentDetails>(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
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
	// TODO: Error mistake in this:
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
				method: 'POST',
				body: JSON.stringify([key]),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}
}
