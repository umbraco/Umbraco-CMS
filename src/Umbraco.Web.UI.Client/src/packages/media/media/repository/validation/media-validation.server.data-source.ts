import type { UmbMediaDetailModel } from '../../types.js';
import {
	type CreateMediaRequestModel,
	MediaService,
	type UpdateMediaRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for Media Validation
 */
export class UmbMediaValidationServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Validate a new Media on the server
	 * @param {UmbMediaDetailModel} model - Media Model
	 * @param {UmbEntityUnique} parentUnique - Parent Unique
	 * @returns {*}
	 */
	async validateCreate(
		model: UmbMediaDetailModel,
		parentUnique: UmbEntityUnique = null,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model) throw new Error('Media is missing');
		if (!model.unique) throw new Error('Media unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateMediaRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			mediaType: { id: model.mediaType.unique },
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		const { data, error } = await tryExecute(
			this.#host,
			MediaService.postMediaValidate({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}

	/**
	 * Validate a existing Media
	 * @param {UmbMediaDetailModel} model - Media Model
	 * @param {Array<UmbVariantId>} variantIds - Variant Ids
	 * @returns {Promise<*>} - The response from the server
	 */
	async validateUpdate(
		model: UmbMediaDetailModel,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		variantIds: Array<UmbVariantId>,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model.unique) throw new Error('Unique is missing');

		//const cultures = variantIds.map((id) => id.culture).filter((culture) => culture !== null) as Array<string>;

		// TODO: make data mapper to prevent errors
		const body: UpdateMediaRequestModel = {
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		const { data, error } = await tryExecute(
			this.#host,
			MediaService.putMediaByIdValidate({
				path: { id: model.unique },
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}
}
