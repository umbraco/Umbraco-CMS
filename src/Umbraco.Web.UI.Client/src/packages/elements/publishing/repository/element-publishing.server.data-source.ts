import type { UmbElementVariantPublishModel } from '../types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
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
