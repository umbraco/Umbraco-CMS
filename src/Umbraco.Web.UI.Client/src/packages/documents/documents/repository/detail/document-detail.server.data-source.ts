import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateDocumentRequestModel, UpdateDocumentRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource implements UmbDetailDataSource<UmbDocumentDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentUnique
	 * @return { UmbDocumentDetailModel }
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: UmbId.new(),
			urls: [],
			template: null,
			documentType: {
				unique: 'documentTypeId',
			},
			isTrashed: false,
			values: [],
			variants: [
				{
					state: null,
					culture: null,
					segment: null,
					name: '',
					publishDate: null,
					createDate: null,
					updateDate: null,
				},
			],
		};

		return { data };
	}

	/**
	 * Fetches a Document with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, DocumentResource.getDocumentById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const document: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: data.id,
			values: data.values,
			variants: data.variants,
			urls: data.urls,
			template: data.template ? { id: data.template.id } : null,
			documentType: { unique: data.documentType.id },
			isTrashed: data.isTrashed,
		};

		return { data: document };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {UmbDocumentDetailModel} model
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async create(model: UmbDocumentDetailModel) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDocumentRequestModel = {
			id: model.unique,
			parent: model.parentUnique,
			documentType: { id: model.documentType.unique },
			template: model.template,
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentResource.postDocument({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Document on the server
	 * @param {UmbDocumentDetailModel} Document
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async update(model: UmbDocumentDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateDocumentRequestModel = {
			template: model.template,
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentResource.putDocumentById({
				id: model.unique,
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Deletes a Document on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		// TODO: update to delete when implemented
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id: unique }));
	}
}
