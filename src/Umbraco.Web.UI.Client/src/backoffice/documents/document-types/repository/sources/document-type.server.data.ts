import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DocumentTypeResource,
	ProblemDetailsModel,
	DocumentTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Type that fetches data from the server
 * @export
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeServerDataSource implements UmbDataSource<DocumentTypeResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Document with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async get(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeByKey({
				key,
			})
		);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
		const data: DocumentTypeResponseModel = {
			properties: [],
		};

		return { data };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {Document} document
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async insert(document: DocumentTypeResponseModel) {
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
		//return tryExecuteAndNotify(this.#host, DocumentTypeResource.postDocument(payload));
		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentTypeResponseModel>(
			this.#host,
			fetch('/umbraco/management/api/v1/document-type', {
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
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(document: DocumentTypeResponseModel) {
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

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentTypeResponseModel>(
			this.#host,
			fetch(`/umbraco/management/api/v1/document-type/${document.key}`, {
				method: 'PUT',
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
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentTypeResponseModel>(
			this.#host,
			fetch(`/umbraco/management/api/v1/document-type/${key}`, {
				method: 'DELETE',
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
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		let problemDetails: ProblemDetailsModel | undefined = undefined;

		try {
			await fetch('/umbraco/management/api/v1/document-type/trash', {
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

		// TODO: use resources when end point is ready:
		/*
		return tryExecuteAndNotify(
			this.#host,
		);
		*/
	}

	/**
	 * Get the allowed document types for a given parent key
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async getAllowedChildrenOf(key: string) {
		if (!key) throw new Error('Key is missing');

		let problemDetails: ProblemDetailsModel | undefined = undefined;
		let data = undefined;

		try {
			const res = await fetch(`/umbraco/management/api/v1/document-type/allowed-children-of/${key}`, {
				method: 'GET',
				headers: {
					'Content-Type': 'application/json',
				},
			});
			data = await res.json();
		} catch (error) {
			problemDetails = { title: `Get allowed children of ${key} failed` };
		}

		return { data, error: problemDetails };
	}
}
