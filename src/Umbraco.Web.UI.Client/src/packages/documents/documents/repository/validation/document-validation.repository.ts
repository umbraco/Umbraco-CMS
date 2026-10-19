import type { UmbDocumentDetailModel } from '../../types.js';
import { UmbDocumentValidationServerDataSource } from './document-validation.server.data-source.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbContentValidationRepository } from '@umbraco-cms/backoffice/content';

type DetailModelType = UmbDocumentDetailModel;

export class UmbDocumentValidationRepository
	extends UmbRepositoryBase
	implements UmbContentValidationRepository<DetailModelType>
{
	#validationDataSource: UmbDocumentValidationServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#validationDataSource = new UmbDocumentValidationServerDataSource(this);
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {DetailModelType} model - The document data to validate.
	 * @param {string | null} [parentUnique] - The unique identifier of the parent document.
	 * @returns {Promise<UmbRepositoryResponse<string>>} The validation result.
	 * @memberof UmbDocumentValidationRepository
	 */
	async validateCreate(model: DetailModelType, parentUnique: string | null): Promise<UmbRepositoryResponse<string>> {
		if (!model) throw new Error('Data is missing');

		return this.#validationDataSource.validateCreate(model, parentUnique);
	}

	/**
	 * Saves the given data
	 * @param {DetailModelType} model - The document data to validate.
	 * @param {Array<UmbVariantId>} variantIds - The variants to validate.
	 * @returns {Promise<UmbRepositoryResponse<string>>} The validation result.
	 * @memberof UmbDocumentValidationRepository
	 */
	async validateSave(model: DetailModelType, variantIds: Array<UmbVariantId>): Promise<UmbRepositoryResponse<string>> {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');

		return this.#validationDataSource.validateUpdate(model, variantIds);
	}
}

export { UmbDocumentValidationRepository as api };
