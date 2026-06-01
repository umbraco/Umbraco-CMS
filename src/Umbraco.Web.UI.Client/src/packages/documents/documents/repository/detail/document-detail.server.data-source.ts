import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateAndPublishDocumentRequestModel,
	CreateDocumentRequestModel,
	UpdateAndPublishDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbDeepMerge, type UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import { UmbDocumentTypeDetailServerDataSource } from '@umbraco-cms/backoffice/document-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A data source for the Document that fetches data from the server
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbDocumentDetailModel>
{
	/**
	 * Creates a new Document scaffold
	 * @param preset
	 * @returns { UmbDocumentDetailModel }
	 * @memberof UmbDocumentServerDataSource
	 */
	async createScaffold(preset: UmbDeepPartialObject<UmbDocumentDetailModel> = {}) {
		const documentTypeUnique = preset.documentType?.unique;

		if (!documentTypeUnique) {
			throw new Error('Document type unique is missing');
		}

		// TODO: investigate if we can use the repository here instead
		const { data } = await new UmbDocumentTypeDetailServerDataSource(this).read(documentTypeUnique);
		const documentTypeIcon = data?.icon ?? null;
		const documentTypeCollection = data?.collection ?? null;

		const defaultData: UmbDocumentDetailModel = {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			unique: UmbId.new(),
			template: null,
			documentType: {
				unique: documentTypeUnique,
				collection: documentTypeCollection,
				icon: documentTypeIcon,
			},
			isTrashed: false,
			values: [],
			variants: [],
			flags: [],
		};

		const scaffold = umbDeepMerge(preset, defaultData);

		return { data: scaffold };
	}

	/**
	 * Fetches a Document with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this, DocumentService.getDocumentById({ path: { id: unique } }));

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
					flags: variant.flags,
				};
			}),
			template: data.template ? { unique: data.template.id } : null,
			documentType: {
				unique: data.documentType.id,
				collection: data.documentType.collection ? { unique: data.documentType.collection.id } : null,
				icon: data.documentType.icon,
			},
			isTrashed: data.isTrashed,
			flags: data.flags,
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

		const body: CreateDocumentRequestModel = this.#mapCreateRequestBody(model, parentUnique);

		const { data, error } = await tryExecute(
			this,
			DocumentService.postDocument({
				body: body,
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

		const body: UpdateDocumentRequestModel = this.#mapUpdateRequestBody(model);

		const { error } = await tryExecute(
			this,
			DocumentService.putDocumentById({
				path: { id: model.unique },
				body: body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Creates and publishes a new Document on the server in a single operation
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after creating
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async createAndPublish(
		model: UmbDocumentDetailModel,
		variantIds: Array<UmbVariantId>,
		parentUnique: string | null = null,
	) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

		const body: CreateAndPublishDocumentRequestModel = {
			...this.#mapCreateRequestBody(model, parentUnique),
			culturesToPublish: this.#mapCulturesToPublish(variantIds),
		};

		const { data, error } = await tryExecute(
			this,
			DocumentService.postDocumentCreateAndPublish({
				body: body,
			}),
		);

		// 201 Created returns only the key (no document body). The workspace reloads after this to refresh
		// its state, so we deliberately do NOT re-read the full document here — that would be a redundant
		// round-trip on top of the reload.
		return { data, error };
	}

	/**
	 * Updates and publishes a Document on the server in a single operation
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after updating
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async updateAndPublish(model: UmbDocumentDetailModel, variantIds: Array<UmbVariantId>) {
		if (!model.unique) throw new Error('Unique is missing');

		const body: UpdateAndPublishDocumentRequestModel = {
			...this.#mapUpdateRequestBody(model),
			culturesToPublish: this.#mapCulturesToPublish(variantIds),
		};

		const { error } = await tryExecute(
			this,
			DocumentService.putDocumentByIdUpdateAndPublish({
				path: { id: model.unique },
				body: body,
			}),
		);

		// 200 returns only a notification header (no document body). The workspace reloads after this to
		// refresh its state, so we deliberately do NOT re-read the full document here — that would be a
		// redundant round-trip on top of the reload.
		return { error };
	}

	/**
	 * Maps the selected variants to the culture codes accepted by the create/update-and-publish endpoints.
	 * Invariant content types require an empty array (cultures cannot be specified), and the server rejects
	 * `null`/`"*"` entries, so invariant variants are filtered out and only distinct culture codes remain.
	 * @param {Array<UmbVariantId>} variantIds - The selected variants to publish
	 * @returns {Array<string>} The distinct culture codes to publish
	 */
	#mapCulturesToPublish(variantIds: Array<UmbVariantId>): Array<string> {
		return [...new Set(variantIds.filter((x) => !x.isCultureInvariant()).map((x) => x.toCultureString()))];
	}

	// TODO: make data mapper to prevent errors
	#mapCreateRequestBody(model: UmbDocumentDetailModel, parentUnique: string | null): CreateDocumentRequestModel {
		return {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};
	}

	// TODO: make data mapper to prevent errors
	#mapUpdateRequestBody(model: UmbDocumentDetailModel): UpdateDocumentRequestModel {
		return {
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};
	}

	/**
	 * Deletes a Document on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this, DocumentService.deleteDocumentById({ path: { id: unique } }));
	}
}
