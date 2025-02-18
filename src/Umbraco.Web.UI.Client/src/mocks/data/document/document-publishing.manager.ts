import type { UmbMockDocumentModel } from './document.data.js';
import type { UmbDocumentMockDB } from './document.db.js';
import type {
	PublishDocumentRequestModel,
	UnpublishDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockDocumentPublishingManager {
	#documentDb: UmbDocumentMockDB;

	constructor(documentDb: UmbDocumentMockDB) {
		this.#documentDb = documentDb;
	}

	publish(id: string, data: PublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

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

			const variant = document.variants.find((x) => x.culture === culture.culture);
			if (variant) {
				variant.state = DocumentVariantStateModel.PUBLISHED;
				variant.scheduledPublishDate = publishTime;
				variant.scheduledUnpublishDate = unpublishTime;
				variant.updateDate = new Date().toISOString();
			}
		});

		this.#documentDb.detail.update(id, document);
	}

	unpublish(id: string, data: UnpublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		if (data.cultures) {
			data.cultures.forEach((culture) => {
				const variant = document.variants.find((x) => x.culture === culture);

				if (variant) {
					variant.state = DocumentVariantStateModel.DRAFT;
					variant.scheduledPublishDate = null;
					variant.scheduledUnpublishDate = null;
					variant.updateDate = new Date().toISOString();
				}
			});
		} else {
			document.variants.forEach((variant) => {
				variant.state = DocumentVariantStateModel.DRAFT;
				variant.scheduledPublishDate = null;
				variant.scheduledUnpublishDate = null;
				variant.updateDate = new Date().toISOString();
			});
		}

		this.#documentDb.detail.update(id, document);
	}
}
