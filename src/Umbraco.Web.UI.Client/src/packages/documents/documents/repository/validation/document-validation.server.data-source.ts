import type { UmbDocumentDetailModel } from '../../types.js';
import {
	type CreateDocumentRequestModel,
	DocumentService,
	type UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for Document Validation

 * @class UmbDocumentPublishingServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentValidationServerDataSource {
	//#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentPublishingServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentPublishingServerDataSource
	 */
	// TODO: [v15]: ignoring unused var here here to prevent a breaking change
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	constructor(host: UmbControllerHost) {
		//this.#host = host;
	}

	/**
	 * Validate a new Document on the server
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @returns {*}
	 */
	async validateCreate(model: UmbDocumentDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

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
	 * @returns {*}
	 */
	async validateUpdate(model: UmbDocumentDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateDocumentRequestModel = {
			template: model.template ? { id: model.template.unique } : null,
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		return tryExecute(
			//this.#host,
			DocumentService.putDocumentByIdValidate({
				id: model.unique,
				requestBody,
			}),
		);
	}
}
