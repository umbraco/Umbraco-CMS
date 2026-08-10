import type { UmbMockElementModel } from '../data/mock-data-set.types.js';
import type { UmbElementMockDB } from './element.db.js';
import type {
	CreateAndPublishElementRequestModel,
	PublishElementRequestModel,
	UnpublishElementRequestModel,
	UpdateAndPublishElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';

export class UmbMockElementPublishingManager {
	#elementDb: UmbElementMockDB;

	constructor(elementDb: UmbElementMockDB) {
		this.#elementDb = elementDb;
	}

	createAndPublish(data: CreateAndPublishElementRequestModel) {
		const id = this.#elementDb.detail.create(data);
		this.#publishCultures(id, data.culturesToPublish);
		return id;
	}

	updateAndPublish(id: string, data: UpdateAndPublishElementRequestModel) {
		this.#elementDb.detail.update(id, data);
		this.#publishCultures(id, data.culturesToPublish);
	}

	#publishCultures(id: string, culturesToPublish: Array<string>) {
		const element: UmbMockElementModel = this.#elementDb.detail.read(id);

		// Invariant content types publish with an empty cultures array; publish the invariant variant in that case.
		const cultures: Array<string | null> = culturesToPublish.length > 0 ? culturesToPublish : [null];

		cultures.forEach((culture) => {
			const variant = element.variants.find((x) => x.culture === culture);
			if (variant) {
				variant.state = UmbElementVariantState.PUBLISHED;
			}
		});

		this.#elementDb.detail.update(id, element);
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
				variant.state = UmbElementVariantState.PUBLISHED;
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
					variant.state = UmbElementVariantState.DRAFT;
				}
			});
		} else {
			element.variants.forEach((variant) => {
				variant.state = UmbElementVariantState.DRAFT;
			});
		}

		this.#elementDb.detail.update(id, element);
	}
}
