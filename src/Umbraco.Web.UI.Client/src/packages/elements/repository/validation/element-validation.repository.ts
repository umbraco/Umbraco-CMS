import type { UmbElementDetailModel } from '../../types.js';
import { UmbElementValidationServerDataSource } from './element-validation.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbContentValidationRepository } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

type DetailModelType = UmbElementDetailModel;

export class UmbElementValidationRepository
	extends UmbRepositoryBase
	implements UmbContentValidationRepository<DetailModelType>
{
	#validationDataSource: UmbElementValidationServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#validationDataSource = new UmbElementValidationServerDataSource(this);
	}

	/**
	 * Performs server-side validation before creating an element.
	 * @param {DetailModelType} model The element model to validate.
	 * @param {string | null} [parentUnique] Optional unique identifier of the parent element.
	 * @returns {*} A promise resolving to the server-side validation result/response.
	 */
	async validateCreate(model: DetailModelType, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');

		return this.#validationDataSource.validateCreate(model, parentUnique);
	}

	/**
	 * Validates the given data for saving; does not persist any changes.
	 * @param {DetailModelType} model
	 * @param variantIds
	 * @returns {*}
	 */
	async validateSave(model: DetailModelType, variantIds: Array<UmbVariantId>) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');

		return this.#validationDataSource.validateUpdate(model, variantIds);
	}
}

export { UmbElementValidationRepository as api };
