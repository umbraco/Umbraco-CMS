import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentTypeRequestModel,
	UpdateDocumentTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbPropertyContainerTypes, UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';

/**
 * A data source for the Document Type that fetches data from the server
 * @class UmbDocumentTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentTypeDetailServerDataSource implements UmbDetailDataSource<UmbDocumentTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

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
	 * Fetches a Media Type with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentTypeService.getDocumentTypeById({ id: unique }),
		);

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const DocumentType: UmbDocumentTypeDetailModel = {
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

		return { data: DocumentType };
	}

	/**
	 * Inserts a new Media Type on the server
	 * @param {UmbDocumentTypeDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async create(model: UmbDocumentTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Media Type is missing');
		if (!model.unique) throw new Error('Media Type unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDocumentTypeRequestModel = {
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
					id: property.id,
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

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentTypeService.postDocumentType({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
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
		const requestBody: UpdateDocumentTypeRequestModel = {
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
					id: property.id,
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

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DocumentTypeService.putDocumentTypeById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Media Type on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeService.deleteDocumentTypeById({
				id: unique,
			}),
		);
	}
}
