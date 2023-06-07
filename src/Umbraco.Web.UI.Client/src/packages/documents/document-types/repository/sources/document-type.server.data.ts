import type { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { CreateDocumentTypeRequestModel, DocumentTypeResource, DocumentTypeResponseModel, UpdateDocumentTypeRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';

/**
 * A data source for the Document Type that fetches data from the server
 * @export
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeServerDataSource implements UmbDataSource<CreateDocumentTypeRequestModel, any, UpdateDocumentTypeRequestModel, DocumentTypeResponseModel> {
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
		// TODO: Type hack to append $type and parentId to the DocumentTypeResponseModel.
		//, parentId: string | null
		const data: DocumentTypeResponseModel & {$type: string} = {
			$type: 'string',
			id: UmbId.new(),
			//parentId: parentId,
			name: '',
			alias: 'new-document-type',
			description: '',
			icon: 'umb:document',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			allowedContentTypes: [],
			compositions: [],
			allowedTemplateIds: [],
			defaultTemplateId: null,
			cleanup: {
				preventCleanup: false,
				keepAllVersionsNewerThanDays: null,
				keepLatestVersionPerDayForDays: null,
			},
			properties: [],
			containers: [],
		};

		return { data };
	}




	/**
	 * Inserts a new Document Type on the server
	 * @param {CreateDocumentTypeRequestModel} documentType
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async insert(documentType: CreateDocumentTypeRequestModel) {
		if (!documentType) throw new Error('Document is missing');
		//if (!document.id) throw new Error('ID is missing');

		documentType = {...documentType};

		// TODO: Hack to remove some props that ruins the document-type post end-point.
		(documentType as any).$type = undefined;
		(documentType as any).id = undefined;

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.postDocumentType({
				requestBody: documentType,
			}),
		);
	}

	/**
	 * Updates a Document Type on the server
	 * @param {string} id
	 * @param {Document} documentType
	 * @return {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async update(id: string, documentType: UpdateDocumentTypeRequestModel) {
		if (!id) throw new Error('Id is missing');

		documentType = {...documentType};

		// TODO: Hack to remove some props that ruins the document-type post end-point.
		(documentType as any).$type = undefined;
		(documentType as any).id = undefined;

		return tryExecuteAndNotify(this.#host, DocumentTypeResource.putDocumentTypeById({ id, requestBody: documentType }));
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

		// TODO: Hack the type to avoid type-error here:
		return tryExecuteAndNotify(this.#host, DocumentTypeResource.deleteDocumentTypeById({ id })) as any;
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
