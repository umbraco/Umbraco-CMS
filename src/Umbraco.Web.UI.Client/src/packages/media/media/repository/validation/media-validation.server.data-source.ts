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

/**
 * A server data source for Media Validatiom
 */
export class UmbMediaValidationServerDataSource {
	//#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPublishingServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	// TODO: [v15]: ignoring unused var here here to prevent a breaking change
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	constructor(host: UmbControllerHost) {
		//this.#host = host;
	}

	/**
	 * Validate a new Media on the server
	 * @param {UmbMediaDetailModel} model - Media Model
	 * @param {UmbEntityUnique} parentUnique - Parent Unique
	 * @returns {*}
	 */
	async validateCreate(model: UmbMediaDetailModel, parentUnique: UmbEntityUnique = null) {
		if (!model) throw new Error('Media is missing');
		if (!model.unique) throw new Error('Media unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMediaRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			mediaType: { id: model.mediaType.unique },
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		return tryExecute(
			//this.#host,
			MediaService.postMediaValidate({
				requestBody,
			}),
		);
	}

	/**
	 * Validate a existing Media
	 * @param {UmbMediaDetailModel} model - Media Model
	 * @param {Array<UmbVariantId>} variantIds - Variant Ids
	 * @returns {Promise<*>} - The response from the server
	 */
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	async validateUpdate(model: UmbMediaDetailModel, variantIds: Array<UmbVariantId>) {
		if (!model.unique) throw new Error('Unique is missing');

		//const cultures = variantIds.map((id) => id.culture).filter((culture) => culture !== null) as Array<string>;

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateMediaRequestModel = {
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		return tryExecute(
			//this.#host,
			MediaService.putMediaByIdValidate({
				id: model.unique,
				requestBody,
			}),
		);
	}
}
