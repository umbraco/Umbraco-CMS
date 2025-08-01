import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_PROPERTY_VALUE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDataSourceResponse, UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentBlueprintRequestModel,
	DocumentBlueprintResponseModel,
	UpdateDocumentBlueprintRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @class UmbDocumentBlueprintServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbDocumentBlueprintServerDataSource implements UmbDetailDataSource<UmbDocumentBlueprintDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentBlueprintServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Document scaffold
	 * @param preset
	 * @returns { UmbDocumentBlueprintDetailModel }
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async createScaffold(preset: Partial<UmbDocumentBlueprintDetailModel> = {}) {
		const data: UmbDocumentBlueprintDetailModel = {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			unique: UmbId.new(),
			documentType: {
				unique: '',
				collection: null,
			},
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
	createVariantScaffold(): UmbDocumentBlueprintVariantModel {
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
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbDocumentBlueprintDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.getDocumentBlueprintById({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		const document = this.#createDocumentBlueprintDetailModel(data);

		return { data: document };
	}

	async scaffoldByUnique(unique: string): Promise<UmbDataSourceResponse<UmbDocumentBlueprintDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.getDocumentBlueprintByIdScaffold({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		const document = this.#createDocumentBlueprintDetailModel(data);

		return { data: document };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {UmbDocumentBlueprintDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async create(model: UmbDocumentBlueprintDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateDocumentBlueprintRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.postDocumentBlueprint({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Document on the server
	 * @param {UmbDocumentBlueprintDetailModel} Document
	 * @param model
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async update(model: UmbDocumentBlueprintDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateDocumentBlueprintRequestModel = {
			values: model.values,
			variants: model.variants,
		};

		const { error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.putDocumentBlueprintById({
				path: { id: model.unique },
				body,
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
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(this.#host, DocumentBlueprintService.deleteDocumentBlueprintById({ path: { id: unique } }));
	}

	#createDocumentBlueprintDetailModel(data: DocumentBlueprintResponseModel): UmbDocumentBlueprintDetailModel {
		return {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			unique: data.id,
			values: data.values.map((value) => {
				return {
					editorAlias: value.editorAlias,
					entityType: UMB_DOCUMENT_BLUEPRINT_PROPERTY_VALUE_ENTITY_TYPE,
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
					scheduledUnpublishDate: variant.scheduledUnpublishDate || null
				};
			}),
			documentType: {
				unique: data.documentType.id,
				collection: data.documentType.collection ? { unique: data.documentType.collection.id } : null,
			},
		};
	}
}
