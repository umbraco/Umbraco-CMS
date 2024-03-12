import type { UmbDocumentBlueprintDetailModel, UmbDocumentBlueprintVariantModel } from '../../types.js';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentBlueprintServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentBlueprintServerDataSource implements UmbDetailDataSource<UmbDocumentBlueprintDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentBlueprintServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Document scaffold
	 * @return { UmbDocumentBlueprintDetailModel }
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async createScaffold(preset: Partial<UmbDocumentBlueprintDetailModel> = {}) {
		const data: UmbDocumentBlueprintDetailModel = {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			unique: UmbId.new(),
			urls: [],
			template: null,
			documentType: {
				unique: '',
				collection: null,
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
	 * @return {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, DocumentResource.getDocumentById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const document: UmbDocumentBlueprintDetailModel = {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			unique: data.id,
			values: data.values.map((value) => {
				return {
					alias: value.alias,
					culture: value.culture || null,
					segment: value.segment || null,
					value: value.value,
				};
			}),
			variants: data.variants.map((variant) => {
				return {
					state: variant.state,
					culture: variant.culture || null,
					segment: variant.segment || null,
					name: variant.name,
					publishDate: variant.publishDate || null,
					createDate: variant.createDate,
					updateDate: variant.updateDate,
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
			},
			isTrashed: data.isTrashed,
		};

		return { data: document };
	}

	/**
	 * Inserts a new Document on the server
	 * @param {UmbDocumentBlueprintDetailModel} model
	 * @return {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async create(model: UmbDocumentBlueprintDetailModel, parentUnique: string | null = null) {
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
	 * @param {UmbDocumentBlueprintDetailModel} Document
	 * @return {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async update(model: UmbDocumentBlueprintDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateDocumentRequestModel = {
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			DocumentResource.putDocumentById({
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
	 * Deletes a Document on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		// TODO: update to delete when implemented
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id: unique }));
	}
}
