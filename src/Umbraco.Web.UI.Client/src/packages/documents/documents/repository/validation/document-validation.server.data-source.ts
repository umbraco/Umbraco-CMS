import type { UmbDocumentDetailModel } from '../../types.js';
import {
	type CreateDocumentRequestModel,
	DocumentService,
	type ValidateUpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

/**
 * A server data source for Document Validation
 */
export class UmbDocumentValidationServerDataSource {
	//#host: UmbControllerHost;

	// TODO: [v15]: ignoring unused var here here to prevent a breaking change
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	constructor(host: UmbControllerHost) {
		//this.#host = host;
	}

	/**
	 * Validate a new Document on the server
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @param parentUnique - Parent Unique
	 * @returns {*}
	 */
	async validateCreate(model: UmbDocumentDetailModel, parentUnique: UmbEntityUnique = null) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDocumentRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			documentType: { id: model.documentType.unique },
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		return tryExecute(
			//this.#host,
			DocumentService.postDocumentValidate({
				requestBody,
			}),
		);
	}

	/**
	 * Validate a existing Document
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @param {Array<UmbVariantId>} variantIds - Variant Ids
	 * @returns {*}
	 */
	async validateUpdate(model: UmbDocumentDetailModel, variantIds: Array<UmbVariantId>) {
		if (!model.unique) throw new Error('Unique is missing');

		const cultures = variantIds.map((id) => id.culture).filter((culture) => culture !== null) as Array<string>;

		// TODO: make data mapper to prevent errors
		const requestBody: ValidateUpdateDocumentRequestModel = {
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
			cultures,
		};

		// Maybe use: tryExecuteAndNotify
		return tryExecute(
			//this.#host,
			DocumentService.putUmbracoManagementApiV11DocumentByIdValidate11({
				id: model.unique,
				requestBody,
			}),
		);
	}
}
