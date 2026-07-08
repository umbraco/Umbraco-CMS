import type { UmbElementVariantPublishModel } from '../types.js';
import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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

	/**
	 * Get the published Element by its unique
	 * @param {string} unique - Element unique
	 * @returns {Promise<UmbDataSourceResponse<UmbElementDetailModel>>} Published element
	 * @memberof UmbElementPublishingServerDataSource
	 */
	async published(unique: string): Promise<UmbDataSourceResponse<UmbElementDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			ElementService.getElementByIdPublished({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		const element: UmbElementDetailModel = {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
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
			documentType: {
				unique: data.documentType.id,
				collection: null,
				icon: data.documentType.icon,
			},
			isTrashed: data.isTrashed,
			flags: data.flags,
		};

		return { data: element };
	}
}
