import type { UmbMediaDetailModel } from '../../types.js';
import { UmbMediaValidationServerDataSource } from './media-validation.server.data-source.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbContentValidationRepository } from '@umbraco-cms/backoffice/content';

type DetailModelType = UmbMediaDetailModel;

export class UmbMediaValidationRepository
	extends UmbRepositoryBase
	implements UmbContentValidationRepository<DetailModelType>
{
	#validationDataSource: UmbMediaValidationServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#validationDataSource = new UmbMediaValidationServerDataSource(this);
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {DetailModelType} model - The model to validate
	 * @param {string | null} [parentUnique] - The parent unique
	 * @returns {*}
	 */
	async validateCreate(model: DetailModelType, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');

		return this.#validationDataSource.validateCreate(model, parentUnique);
	}

	/**
	 * Saves the given data
	 * @param {DetailModelType} model - The model to save
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to save
	 * @returns {*}
	 */
	async validateSave(model: DetailModelType, variantIds: Array<UmbVariantId>) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');

		return this.#validationDataSource.validateUpdate(model, variantIds);
	}
}

export { UmbMediaValidationRepository as api };
