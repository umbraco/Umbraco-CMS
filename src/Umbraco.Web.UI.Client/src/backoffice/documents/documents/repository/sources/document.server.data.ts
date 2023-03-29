import { v4 as uuidv4 } from 'uuid';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DocumentResource,
	ProblemDetailsModel,
	DocumentResponseModel,
	ContentStateModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource implements UmbDataSource<DocumentResponseModel> {
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
	 * @memberof UmbDocumentServerDataSource
	 */
	async get(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentByKey({
				key,
			})
		);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(documentTypeKey: string) {
		const data: DocumentResponseModel = {
			urls: [],
			templateKey: null,
			key: uuidv4(),
			contentTypeKey: documentTypeKey,
			values: [],
			variants: [
				{
					$type: '',
					state: ContentStateModel.DRAFT,
					publishDate: null,
					culture: null,
					segment: null,
					name: '',
					createDate: new Date().toISOString(),
					updateDate: undefined,
				},
			],
		};

		return { data };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {Document} document
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async insert(document: DocumentResponseModel) {
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
		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentResponseModel>(
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
	 * @memberof UmbDocumentServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(document: DocumentResponseModel) {
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
		return tryExecuteAndNotify<DocumentResponseModel>(
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
	 * @memberof UmbDocumentServerDataSource
	 */
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentResponseModel>(
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
	 * Deletes a Document on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		let problemDetails: ProblemDetailsModel | undefined = undefined;

		try {
			await fetch('/umbraco/management/api/v1/document/trash', {
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

		/* TODO: use resources when end point is ready:
		return tryExecuteAndNotify(this.#host);
		*/
	}
}
