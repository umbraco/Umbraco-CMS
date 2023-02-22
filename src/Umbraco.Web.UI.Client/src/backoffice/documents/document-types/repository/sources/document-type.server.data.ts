import { RepositoryDetailDataSource } from '@umbraco-cms/repository';
import { DocumentTypeResource, ProblemDetailsModel, DocumentTypeModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Document Type that fetches data from the server
 * @export
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeServerDataSource implements RepositoryDetailDataSource<DocumentTypeModel> {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
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
		const data: DocumentTypeModel = {
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
	async insert(document: DocumentTypeModel) {
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
		return tryExecuteAndNotify<DocumentTypeModel>(
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
	async update(document: DocumentTypeModel) {
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
		return tryExecuteAndNotify<DocumentTypeModel>(
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
		return tryExecuteAndNotify<DocumentTypeModel>(
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
	// TODO: Error mistake in this:
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/document-type/${key}`, {
				method: 'DELETE',
				body: JSON.stringify([key]),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}
}
