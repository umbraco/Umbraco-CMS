import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DocumentResource,
	DocumentResponseModel,
	ContentStateModel,
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource
	implements UmbDataSource<CreateDocumentRequestModel, any, UpdateDocumentRequestModel, DocumentResponseModel>
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
			throw new Error('Id is missing');
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
	async createScaffold(documentTypeId: string, preset?: Partial<CreateDocumentRequestModel>) {
		const data: DocumentResponseModel = {
			urls: [],
			templateId: null,
			id: UmbId.new(),
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
			...preset,
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
			throw new Error('Id is missing');
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
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
				method: 'POST',
				body: JSON.stringify([id]),
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json())
		);
	}
}
