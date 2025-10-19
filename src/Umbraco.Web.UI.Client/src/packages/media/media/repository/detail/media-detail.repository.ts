import type { UmbMediaDetailModel } from '../../types.js';
import { UmbMediaServerDataSource } from './media-detail.server.data-source.js';
import { UMB_MEDIA_DETAIL_STORE_CONTEXT } from './media-detail.store.context-token.js';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaDetailRepository extends UmbDetailRepositoryBase<UmbMediaDetailModel> {
	#imagingRepository = new UmbImagingRepository(this);
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaServerDataSource, UMB_MEDIA_DETAIL_STORE_CONTEXT);
	}

	/**
	 * @inheritdoc
	 */
	override async save(model: UmbMediaDetailModel) {
		const result = await super.save(model);

		if (!result.error) {
			// Attempt to clear image cache
			await this.#imagingRepository._internal_clearCropByUnique(model.unique);
		}

		return result;
	}

	/**
	 * @inheritdoc
	 */
	override async delete(unique: string) {
		const result = await super.delete(unique);

		if (!result.error) {
			// Attempt to clear image cache
			await this.#imagingRepository._internal_clearCropByUnique(unique);
		}

		return result;
	}
}

export { UmbMediaDetailRepository as api };

export default UmbMediaDetailRepository;
