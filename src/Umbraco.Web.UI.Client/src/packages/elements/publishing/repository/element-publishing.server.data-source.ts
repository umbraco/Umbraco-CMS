import type { UmbElementVariantPublishModel } from '../types.js';
import type { UmbElementDetailModel } from '../../types.js';
import {
	umbMapElementCreateRequestBody,
	umbMapElementUpdateRequestBody,
} from '../../repository/detail/element-detail-request.mappers.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	CreateAndPublishElementRequestModel,
	UpdateAndPublishElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A server data source for Element publishing
 * @class UmbElementPublishingServerDataSource
 */
export class UmbElementPublishingServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementPublishingServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementPublishingServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates and publishes a new Element on the server in a single operation
	 * @param {UmbElementDetailModel} model - Element Model
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after creating
	 * @param {string | null} parentUnique - The unique of the parent to create under
	 * @returns {*}
	 * @memberof UmbElementPublishingServerDataSource
	 */
	async createAndPublish(
		model: UmbElementDetailModel,
		variantIds: Array<UmbVariantId>,
		parentUnique: string | null = null,
	) {
		if (!model) throw new Error('Element is missing');
		if (!model.unique) throw new Error('Element unique is missing');

		const body: CreateAndPublishElementRequestModel = {
			...umbMapElementCreateRequestBody(model, parentUnique),
			culturesToPublish: this.#mapCulturesToPublish(variantIds),
		};

		// 201 Created returns only the key (no element body). The workspace reloads after this to refresh
		// its state, so we deliberately do NOT re-read the full element here — that would be a redundant
		// round-trip on top of the reload.
		return tryExecute(this.#host, ElementService.postElementCreateAndPublish({ body }));
	}

	/**
	 * Updates and publishes an Element on the server in a single operation
	 * @param {UmbElementDetailModel} model - Element Model
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after updating
	 * @returns {*}
	 * @memberof UmbElementPublishingServerDataSource
	 */
	async updateAndPublish(model: UmbElementDetailModel, variantIds: Array<UmbVariantId>) {
		if (!model.unique) throw new Error('Unique is missing');

		const body: UpdateAndPublishElementRequestModel = {
			...umbMapElementUpdateRequestBody(model),
			culturesToPublish: this.#mapCulturesToPublish(variantIds),
		};

		// 200 returns only a notification header (no element body). The workspace reloads after this to
		// refresh its state, so we deliberately do NOT re-read the full element here.
		return tryExecute(
			this.#host,
			ElementService.putElementByIdUpdateAndPublish({ path: { id: model.unique }, body }),
		);
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

	/**
	 * Publish one or more variants of an Element
	 * @param {string} unique
	 * @param {Array<UmbElementVariantPublishModel>} variants
	 * @returns {*}
	 * @memberof UmbElementPublishingServerDataSource
	 */
	async publish(unique: string, variants: Array<UmbElementVariantPublishModel>) {
		if (!unique) throw new Error('Id is missing');

		const publishSchedules = variants.map((variant) => ({
			culture: variant.variantId.isCultureInvariant() ? null : variant.variantId.toCultureString(),
			schedule: variant.schedule ?? null,
		}));

		return tryExecute(
			this.#host,
			ElementService.putElementByIdPublish({
				path: { id: unique },
				body: { publishSchedules },
			}),
		);
	}

	/**
	 * Unpublish one or more variants of an Element
	 * @param {string} unique
	 * @param {Array<UmbVariantId>} variantIds
	 * @returns {*}
	 * @memberof UmbElementPublishingServerDataSource
	 */
	async unpublish(unique: string, variantIds: Array<UmbVariantId>) {
		if (!unique) throw new Error('Id is missing');

		// If variants are culture invariant, we need to pass null to the API
		const hasInvariant = variantIds.some((variant) => variant.isCultureInvariant());

		if (hasInvariant) {
			return tryExecute(
				this.#host,
				ElementService.putElementByIdUnpublish({
					path: { id: unique },
					body: { cultures: null },
				}),
			);
		}

		return tryExecute(
			this.#host,
			ElementService.putElementByIdUnpublish({
				path: { id: unique },
				body: { cultures: variantIds.map((variant) => variant.toCultureString()) },
			}),
		);
	}
}
