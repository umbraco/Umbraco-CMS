import type { UmbDocumentTypeTemplateModel } from '../../types.js';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Type Template that fetches data from the server
 * @class UmbDocumentTypeTemplateServerDataSource
 */
export class UmbDocumentTypeTemplateServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeTemplateServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeTemplateServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}
	/**
	 * Creates a template for a document type on the server
	 * @param {string} unique - The unique identifier of the document type.
	 * @param {UmbDocumentTypeTemplateModel} model - The template to create.
	 * @returns {Promise<UmbDataSourceResponse<string>>} The unique identifier of the created template.
	 * @memberof UmbDocumentTypeTemplateServerDataSource
	 */
	async createTemplate(unique: string, model: UmbDocumentTypeTemplateModel): Promise<UmbDataSourceResponse<string>> {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentTypeService.postDocumentTypeByIdTemplate({
				path: { id: unique },
				body: {
					name: model.name,
					alias: model.alias,
					isDefault: model.isDefault,
				},
			}),
		);

		if (data) {
			return { data: data as string };
		}

		return { error };
	}
}
