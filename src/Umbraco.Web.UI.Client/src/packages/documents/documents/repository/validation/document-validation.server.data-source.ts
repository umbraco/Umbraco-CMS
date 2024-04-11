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
 * @export
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
	constructor(host: UmbControllerHost) {
		//this.#host = host;
	}

	/**
	 * Validate a new Document on the server
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @return {*}
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
		const { data, error } = await tryExecute(
			//this.#host,
			DocumentService.postDocumentValidate({
				requestBody,
			}),
		);

		if (data) {
			return { data };
		}

		return { error };
	}

	/**
	 * Validate a existing Document
	 * @param {UmbDocumentDetailModel} model - Document Model
	 * @return {*}
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
		const { data, error } = await tryExecute(
			//this.#host,
			DocumentService.putDocumentByIdValidate({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return { data };
		}

		return { error };
	}
}
