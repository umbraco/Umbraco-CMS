import type { UmbDocumentDetailModel, UmbDocumentVariantModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource implements UmbDetailDataSource<UmbDocumentDetailModel> {
	#host: UmbControllerHost;
	#languageRepository;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#languageRepository = new UmbLanguageCollectionRepository(host);
	}

	/**
	 * Creates a new Document scaffold
	 * @param {(string | null)} parentUnique
	 * @return { UmbDocumentDetailModel }
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(parentUnique: string | null, preset: Partial<UmbDocumentDetailModel> = {}) {
		const data: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: UmbId.new(),
			parentUnique: parentUnique,
			urls: [],
			template: null,
			documentType: {
				unique: '',
			},
			isTrashed: false,
			values: [],
			variants: [this.createVariantScaffold()],
			...preset,
		};

		return { data };
	}

	/**
	 * Creates a new variant scaffold.
	 * @returns A new variant scaffold.
	 */
	createVariantScaffold(): UmbDocumentVariantModel {
		return {
			state: null,
			culture: null,
			segment: null,
			name: '',
			publishDate: null,
			createDate: null,
			updateDate: null,
			isMandatory: false,
		};
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
			parentUnique: null, // TODO: this is not correct. It will be solved when we know where to get the parent from
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
					isMandatory: false, // TODO: this is not correct. It will be solved when we know where to get the isMandatory from
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
				collection: data.documentType.collection ?? undefined,
			},
			isTrashed: data.isTrashed,
		};

		// Add missing languages as variants to the document if the document allows variants
		const { data: languages } = await this.#languageRepository.requestCollection({});
		if (!languages) return { data: document };

		const allowVariants = document.variants.length > 1 || document.variants[0].culture !== null;
		if (!allowVariants) return { data: document };

		const missingLanguages = languages.items.filter(
			(language) => !document.variants.some((variant) => variant.culture === language.unique),
		);
		document.variants = document.variants.concat(
			missingLanguages.map((language) => {
				const scaffold = this.createVariantScaffold();
				return {
					...scaffold,
					languageName: language.name,
					culture: language.unique,
					isMandatory: language.isMandatory,
				};
			}),
		);

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
			parent: model.parentUnique ? { id: model.parentUnique } : null,
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
	 * @param {UmbDocumentDetailModel} Document
	 * @return {*}
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
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		// TODO: update to delete when implemented
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id: unique }));
	}
}
