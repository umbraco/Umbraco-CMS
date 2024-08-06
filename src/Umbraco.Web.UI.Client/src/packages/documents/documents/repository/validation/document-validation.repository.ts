import type { UmbDocumentDetailModel } from '../../types.js';
import { UmbDocumentValidationServerDataSource } from './document-validation.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

type DetailModelType = UmbDocumentDetailModel;

export class UmbDocumentValidationRepository extends UmbRepositoryBase {
	#validationDataSource: UmbDocumentValidationServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#validationDataSource = new UmbDocumentValidationServerDataSource(this);
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {DetailModelType} model
	 * @param {string | null} [parentUnique]
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async validateCreate(model: DetailModelType, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');

		return this.#validationDataSource.validateCreate(model, parentUnique);
	}

	/**
	 * Saves the given data
	 * @param {DetailModelType} model
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async validateSave(model: DetailModelType) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');

		return this.#validationDataSource.validateUpdate(model);
	}
}

export { UmbDocumentValidationRepository as api };
