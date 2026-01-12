import type { UmbMockElementModel } from './element.data.js';
import type { UmbElementMockDB } from './element.db.js';
import type {
	PublishElementRequestModel,
	UnpublishElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockElementPublishingManager {
	#elementDb: UmbElementMockDB;

	constructor(elementDb: UmbElementMockDB) {
		this.#elementDb = elementDb;
	}

	publish(id: string, data: PublishElementRequestModel) {
		const element: UmbMockElementModel = this.#elementDb.detail.read(id);

		data.publishSchedules.forEach((culture) => {
			const publishTime = culture.schedule?.publishTime;
			const unpublishTime = culture.schedule?.unpublishTime;

			if (publishTime && new Date(publishTime) < new Date()) {
				throw new Error('Publish date cannot be in the past');
			}

			if (unpublishTime && new Date(unpublishTime) < new Date()) {
				throw new Error('Unpublish date cannot be in the past');
			}

			if (unpublishTime && publishTime && new Date(unpublishTime) < new Date(publishTime)) {
				throw new Error('Unpublish date cannot be before publish date');
			}

			const variant = element.variants.find((x) => x.culture === culture.culture);
			if (variant) {
				variant.state = DocumentVariantStateModel.PUBLISHED;
			}
		});

		this.#elementDb.detail.update(id, element);
	}

	unpublish(id: string, data: UnpublishElementRequestModel) {
		const element: UmbMockElementModel = this.#elementDb.detail.read(id);

		if (data.cultures) {
			data.cultures.forEach((culture) => {
				const variant = element.variants.find((x) => x.culture === culture);

				if (variant) {
					variant.state = DocumentVariantStateModel.DRAFT;
				}
			});
		} else {
			element.variants.forEach((variant) => {
				variant.state = DocumentVariantStateModel.DRAFT;
			});
		}

		this.#elementDb.detail.update(id, element);
	}
}
