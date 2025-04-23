import type { UmbDocumentDetailModel, UmbDocumentVariantPublishModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE } from '../../entity.js';
import type {
	CultureAndScheduleRequestModel,
	PublishDocumentRequestModel,
	PublishDocumentWithDescendantsRequestModel,
	UnpublishDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for Document publishing
 * @class UmbDocumentPublishingServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentPublishingServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPublishingServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Publish one or more variants of a Document
	 * @param {string} unique
	 * @param {Array<UmbVariantId>} variantIds
	 * @param variants
	 * @returns {*}
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async publish(unique: string, variants: Array<UmbDocumentVariantPublishModel>) {
		if (!unique) throw new Error('Id is missing');

		const publishSchedules: CultureAndScheduleRequestModel[] = variants.map<CultureAndScheduleRequestModel>(
			(variant) => {
				return {
					culture: variant.variantId.isCultureInvariant() ? null : variant.variantId.toCultureString(),
					schedule: variant.schedule,
				};
			},
		);

		// TODO: THIS DOES NOT TAKE SEGMENTS INTO ACCOUNT!!!!!!
		const body: PublishDocumentRequestModel = {
			publishSchedules,
		};

		return tryExecute(this.#host, DocumentService.putDocumentByIdPublish({ path: { id: unique }, body: body }));
	}

	/**
	 * Unpublish one or more variants of a Document
	 * @param {string} unique
	 * @param {Array<UmbVariantId>} variantIds
	 * @returns {*}
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async unpublish(unique: string, variantIds: Array<UmbVariantId>) {
		if (!unique) throw new Error('Id is missing');

		// TODO: THIS DOES NOT TAKE SEGMENTS INTO ACCOUNT!!!!!!

		// If variants are culture invariant, we need to pass null to the API
		const hasInvariant = variantIds.some((variant) => variant.isCultureInvariant());

		if (hasInvariant) {
			const body: UnpublishDocumentRequestModel = {
				cultures: null,
			};

			return tryExecute(this.#host, DocumentService.putDocumentByIdUnpublish({ path: { id: unique }, body: body }));
		}

		const body: UnpublishDocumentRequestModel = {
			cultures: variantIds.map((variant) => variant.toCultureString()),
		};

		return tryExecute(this.#host, DocumentService.putDocumentByIdUnpublish({ path: { id: unique }, body: body }));
	}

	/**
	 * Publish variants of a document and all its descendants
	 * @param unique
	 * @param variantIds
	 * @param includeUnpublishedDescendants
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async publishWithDescendants(
		unique: string,
		variantIds: Array<UmbVariantId>,
		includeUnpublishedDescendants: boolean,
	) {
		if (!unique) throw new Error('Id is missing');

		const body: PublishDocumentWithDescendantsRequestModel = {
			cultures: variantIds.map((variant) => variant.toCultureString()),
			includeUnpublishedDescendants,
		};

		// Initiate the publish descendants task and get back a task Id.
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.putDocumentByIdPublishWithDescendants({ path: { id: unique }, body: body }),
		);

		if (error || !data) {
			return { error };
		}

		const taskId = data.taskId;

		// Poll until we know publishing is finished, then return the result.
		let isFirstPoll = true;
		while (true) {
			await new Promise((resolve) => setTimeout(resolve, isFirstPoll ? 1000 : 5000));
			isFirstPoll = false;
			const { data, error } = await tryExecute(
				this.#host,
				DocumentService.getDocumentByIdPublishWithDescendantsResultByTaskId({ path: { id: unique, taskId } }),
			);
			if (error || !data) {
				return { error };
			}

			if (data.isComplete) {
				return { error: null };
			}
		}
	}

	/**
	 * Get the published Document by its unique
	 * @param {string} unique - Document unique
	 * @returns {Promise<UmbDataSourceResponse<UmbDocumentDetailModel>>} Published document
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async published(unique: string): Promise<UmbDataSourceResponse<UmbDocumentDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getDocumentByIdPublished({ path: { id: unique } }),
		);

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
					entityType: UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE,
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
}
