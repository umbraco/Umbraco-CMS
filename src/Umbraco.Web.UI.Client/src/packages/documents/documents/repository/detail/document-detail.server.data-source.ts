import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource implements UmbDetailDataSource<UmbDocumentDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Document scaffold
	 * @param preset
	 * @returns { UmbDocumentDetailModel }
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(preset: Partial<UmbDocumentDetailModel> = {}) {
		const data: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: UmbId.new(),
			urls: [],
			template: null,
			documentType: {
				unique: '',
				collection: null,
				icon: null,
			},
			isTrashed: false,
			values: [],
			variants: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Creates a new variant scaffold.
	 * @returns A new variant scaffold.
	 */
	/*
	// TDOD: remove if not used
	createVariantScaffold(): UmbDocumentVariantModel {
		return {
			state: null,
			culture: null,
			segment: null,
			name: '',
			publishDate: null,
			createDate: null,
			updateDate: null,
		};
	}
	*/

	/**
	 * Fetches a Document with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, DocumentService.getDocumentById({ path: { id: unique } }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const document: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: data.id,
			values: data.values.map((value) => {
				return {
					editorAlias: value.editorAlias,
					culture: value.culture || null,
					segment: value.segment || null,
					alias: value.alias,
					value: value.value,
				};
			}),
			variants: data.variants.map((variant) => {
				return {
					culture: variant.culture || null,
					segment: variant.segment || null,
					state: variant.state,
					name: variant.name,
					publishDate: variant.publishDate || null,
					createDate: variant.createDate,
					updateDate: variant.updateDate,
					scheduledPublishDate: variant.scheduledPublishDate || null,
					scheduledUnpublishDate: variant.scheduledUnpublishDate || null,
				};
			}),
			urls: data.urls.map((url) => {
				return {
					culture: url.culture || null,
					url: url.url,
				};
			}),
			template: data.template ? { unique: data.template.id } : null,
			documentType: {
				unique: data.documentType.id,
				collection: data.documentType.collection ? { unique: data.documentType.collection.id } : null,
				icon: data.documentType.icon,
			},
			isTrashed: data.isTrashed,
		};

		return { data: document };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async create(model: UmbDocumentDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDocumentRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.postDocument({
				body: requestBody,
			}),
		);

		if (data) {
			return this.read(data as any);
		}

		return { error };
	}

	/**
	 * Updates a Document on the server
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async update(model: UmbDocumentDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateDocumentRequestModel = {
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};

		const { error } = await tryExecute(
			this.#host,
			DocumentService.putDocumentById({
				path: { id: model.unique },
				body: requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Document on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, DocumentService.deleteDocumentById({ path: { id: unique } }));
	}
}
