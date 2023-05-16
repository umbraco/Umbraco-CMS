import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { DocumentTypeResource, DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Type that fetches data from the server
 * @export
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeServerDataSource implements UmbDataSource<any, any, any, DocumentTypeResponseModel> {
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
	 * Fetches a Document with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeById({
				id: id,
			})
		);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async createScaffold(parentId: string | null) {
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
		if (!document.id) throw new Error('ID is missing');

		let body: string;

		try {
			body = JSON.stringify(document);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}
		//return tryExecuteAndNotify(this.#host, DocumentTypeResource.postDocument(payload));
		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<string>(
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
	async update(id: string, document: any) {
		if (!id) throw new Error('Id is missing');

		let body: string;

		try {
			body = JSON.stringify(document);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentTypeResponseModel>(
			this.#host,
			fetch(`/umbraco/management/api/v1/document-type/${document.id}`, {
				method: 'PUT',
				body: body,
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
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document-type/trash', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}

	/**
	 * Get the allowed document types for a given parent id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async getAllowedChildrenOf(id: string) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/document-type/allowed-children-of/${id}`, {
				method: 'GET',
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}
}
