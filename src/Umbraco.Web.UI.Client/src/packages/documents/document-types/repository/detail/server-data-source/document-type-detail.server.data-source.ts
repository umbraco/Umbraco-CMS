import type { UmbDocumentTypeDetailModel } from '../../../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../../entity.js';
import { UmbManagementApiDocumentTypeDetailDataRequestManager } from './document-type-detail.server.request-manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentTypeRequestModel,
	DocumentTypeResponseModel,
	UpdateDocumentTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyContainerTypes, UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the Document Type that fetches data from the server
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeDetailServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbDocumentTypeDetailModel>
{
	#detailRequestManager = new UmbManagementApiDocumentTypeDetailDataRequestManager(this);

	/**
	 * Creates a new Document Type scaffold
	 * @param {(string | null)} parentUnique
	 * @param preset
	 * @returns { CreateDocumentTypeRequestModel }
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async createScaffold(preset: Partial<UmbDocumentTypeDetailModel> = {}) {
		const data: UmbDocumentTypeDetailModel = {
			entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			alias: '',
			description: '',
			icon: 'icon-document',
			allowedAtRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			properties: [],
			containers: [],
			allowedContentTypes: [],
			compositions: [],
			allowedTemplates: [],
			defaultTemplate: null,
			cleanup: {
				preventCleanup: false,
				keepAllVersionsNewerThanDays: null,
				keepLatestVersionPerDayForDays: null,
			},
			collection: null,
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Document Type with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#detailRequestManager.read(unique);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Fetches multiple Document Types by their unique IDs from the server
	 * @param {Array<string>} uniques - The unique IDs of the document types to fetch
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async readMany(uniques: Array<string>) {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const { data, error } = await this.#detailRequestManager.readMany(uniques);

		return {
			data: data?.items?.map((item) => this.#mapServerResponseModelToEntityDetailModel(item)),
			error,
		};
	}

	/**
	 * Inserts a new Document Type on the server
	 * @param {UmbDocumentTypeDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async create(model: UmbDocumentTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Document Type is missing');
		if (!model.unique) throw new Error('Document Type unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateDocumentTypeRequestModel = {
			parent: parentUnique ? { id: parentUnique } : null,
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers,
			allowedDocumentTypes: model.allowedContentTypes.map((allowedContentType) => {
				return {
					documentType: { id: allowedContentType.contentType.unique },
					sortOrder: allowedContentType.sortOrder,
				};
			}),
			compositions: model.compositions.map((composition) => {
				return {
					documentType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
			id: model.unique,
			allowedTemplates: model.allowedTemplates,
			defaultTemplate: model.defaultTemplate ? { id: model.defaultTemplate.id } : null,
			cleanup: model.cleanup,
			collection: model.collection?.unique ? { id: model.collection?.unique } : null,
		};

		const { data, error } = await this.#detailRequestManager.create(body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Updates a DocumentType on the server
	 * @param {UmbDocumentTypeDetailModel} DocumentType
	 * @param model
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async update(model: UmbDocumentTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateDocumentTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers.map((container) => {
				return {
					id: container.id,
					parent: container.parent ? { id: container.parent.id } : null,
					name: container.name ?? '',
					type: container.type as UmbPropertyContainerTypes, // TODO: check if the value is valid
					sortOrder: container.sortOrder,
				};
			}),
			allowedDocumentTypes: model.allowedContentTypes.map((allowedContentType) => {
				return {
					documentType: { id: allowedContentType.contentType.unique },
					sortOrder: allowedContentType.sortOrder,
				};
			}),
			compositions: model.compositions.map((composition) => {
				return {
					documentType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
			allowedTemplates: model.allowedTemplates,
			defaultTemplate: model.defaultTemplate ? { id: model.defaultTemplate.id } : null,
			cleanup: model.cleanup,
			collection: model.collection?.unique ? { id: model.collection?.unique } : null,
		};

		const { data, error } = await this.#detailRequestManager.update(model.unique, body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Deletes a Document Type on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#detailRequestManager.delete(unique);
	}

	// TODO: change this to a mapper extension when the endpoints returns a $type for DocumentTypeResponseModel
	#mapServerResponseModelToEntityDetailModel(data: DocumentTypeResponseModel): UmbDocumentTypeDetailModel {
		return {
			entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			alias: data.alias,
			description: data.description ?? '',
			icon: data.icon,
			allowedAtRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			properties: data.properties.map((property) => {
				return {
					id: property.id,
					unique: property.id,
					container: property.container,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { unique: property.dataType.id },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: data.containers as UmbPropertyTypeContainerModel[],
			allowedContentTypes: data.allowedDocumentTypes.map((allowedDocumentType) => {
				return {
					contentType: { unique: allowedDocumentType.documentType.id },
					sortOrder: allowedDocumentType.sortOrder,
				};
			}),
			compositions: data.compositions.map((composition) => {
				return {
					contentType: { unique: composition.documentType.id },
					compositionType: composition.compositionType,
				};
			}),
			allowedTemplates: data.allowedTemplates,
			defaultTemplate: data.defaultTemplate ? { id: data.defaultTemplate.id } : null,
			cleanup: data.cleanup,
			collection: data.collection ? { unique: data.collection?.id } : null,
		};
	}
}
