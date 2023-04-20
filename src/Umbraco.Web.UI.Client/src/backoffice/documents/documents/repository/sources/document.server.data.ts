import { v4 as uuidv4 } from 'uuid';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DocumentResource,
	ProblemDetailsModel,
	DocumentResponseModel,
	ContentStateModel,
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource
	implements UmbDataSource<CreateDocumentRequestModel, UpdateDocumentRequestModel, DocumentResponseModel>
{
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
	 * @memberof UmbDocumentServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentById({
				id,
			})
		);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(documentTypeId: string) {
		const data: DocumentResponseModel = {
			urls: [],
			templateId: null,
			id: uuidv4(),
			contentTypeId: documentTypeId,
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
	async insert(document: CreateDocumentRequestModel & { id: string }) {
		if (!document.id) throw new Error('Id is missing');

		return tryExecuteAndNotify(this.#host, DocumentResource.postDocument({ requestBody: document }));
	}

	/**
	 * Updates a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async update(id: string, document: UpdateDocumentRequestModel) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentById({ id, requestBody: document }));
	}

	/**
	 * Trash a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async trash(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DocumentResponseModel>(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}) as any
		);
	}

	/**
	 * Deletes a Document on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		let problemDetails: ProblemDetailsModel | undefined = undefined;

		try {
			await fetch('/umbraco/management/api/v1/document/trash', {
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

		/* TODO: use resources when end point is ready:
		return tryExecuteAndNotify(this.#host);
		*/
	}
}
