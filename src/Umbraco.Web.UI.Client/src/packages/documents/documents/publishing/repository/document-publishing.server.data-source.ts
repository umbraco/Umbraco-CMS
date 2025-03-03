import type { UmbDocumentDetailModel, UmbDocumentVariantPublishModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type {
	CultureAndScheduleRequestModel,
	PublishDocumentRequestModel,
	PublishDocumentWithDescendantsRequestModel,
	UnpublishDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
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
		const requestBody: PublishDocumentRequestModel = {
			publishSchedules,
		};

		return tryExecuteAndNotify(this.#host, DocumentService.putDocumentByIdPublish({ id: unique, requestBody }));
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
			const requestBody: UnpublishDocumentRequestModel = {
				cultures: null,
			};

			return tryExecuteAndNotify(this.#host, DocumentService.putDocumentByIdUnpublish({ id: unique, requestBody }));
		}

		const requestBody: UnpublishDocumentRequestModel = {
			cultures: variantIds.map((variant) => variant.toCultureString()),
		};

		return tryExecuteAndNotify(this.#host, DocumentService.putDocumentByIdUnpublish({ id: unique, requestBody }));
	}

	/**
	 * Publish variants of a document and all its descendants
	 * @param unique
	 * @param variantIds
	 * @param includeUnpublishedDescendants
	 * @param forceRepublish
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async publishWithDescendants(
		unique: string,
		variantIds: Array<UmbVariantId>,
		includeUnpublishedDescendants: boolean,
		forceRepublish: boolean,
	) {
		if (!unique) throw new Error('Id is missing');

		const requestBody: PublishDocumentWithDescendantsRequestModel = {
			cultures: variantIds.map((variant) => variant.toCultureString()),
			includeUnpublishedDescendants,
			forceRepublish,
		};

		return tryExecuteAndNotify(
			this.#host,
			DocumentService.putDocumentByIdPublishWithDescendants({ id: unique, requestBody }),
		);
	}

	/**
	 * Get the published Document by its unique
	 * @param {string} unique - Document unique
	 * @returns {Promise<UmbDataSourceResponse<UmbDocumentDetailModel>>} Published document
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	async published(unique: string): Promise<UmbDataSourceResponse<UmbDocumentDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentService.getDocumentByIdPublished({ id: unique }),
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
}
