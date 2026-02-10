import type { UmbElementDetailModel } from '../../types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	CreateElementRequestModel,
	ValidateUpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A server data source for Element Validation
 */
export class UmbElementValidationServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Validate a new Element on the server
	 * @param {UmbElementDetailModel} model - Element Model
	 * @param parentUnique - Parent Unique
	 * @returns {*}
	 */
	async validateCreate(
		model: UmbElementDetailModel,
		parentUnique: UmbEntityUnique = null,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model) throw new Error('Element is missing');
		if (!model.unique) throw new Error('Element unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateElementRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		const { data, error } = await tryExecute(
			this.#host,
			ElementService.postElementValidate({
				body,
			}),
			{
				disableNotifications: true,
			},
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}

	/**
	 * Validate a existing Element
	 * @param {UmbElementDetailModel} model - Element Model
	 * @param {Array<UmbVariantId>} variantIds - Variant Ids
	 * @returns {*}
	 */
	async validateUpdate(
		model: UmbElementDetailModel,
		variantIds: Array<UmbVariantId>,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model.unique) throw new Error('Unique is missing');

		const cultures = variantIds.map((id) => id.culture).filter((culture) => culture !== null) as Array<string>;

		// TODO: make data mapper to prevent errors
		const body: ValidateUpdateElementRequestModel = {
			values: model.values,
			variants: model.variants,
			cultures,
		};

		// Maybe use: tryExecuteAndNotify
		const { data, error } = await tryExecute(
			this.#host,
			ElementService.putElementByIdValidate({
				path: { id: model.unique },
				body,
			}),
			{
				disableNotifications: true,
			},
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}
}
