import type { UmbMockDocumentModel } from './document.data.js';
import type { UmbDocumentMockDB } from './document.db.js';
import type { PublishDocumentRequestModel, UnpublishDocumentRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { ContentStateModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockDocumentPublishingManager {
	#documentDb: UmbDocumentMockDB;

	constructor(documentDb: UmbDocumentMockDB) {
		this.#documentDb = documentDb;
	}

	publish(id: string, data: PublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		document?.variants?.forEach((variant) => {
			const hasCulture = variant.culture && data.cultures?.includes(variant.culture);

			if (hasCulture) {
				variant.state = ContentStateModel.PUBLISHED;
			}
		});

		this.#documentDb.detail.update(id, document);
	}

	unpublish(id: string, data: UnpublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		document?.variants?.forEach((variant) => {
			const hasCulture = variant.culture && data.culture === variant.culture;

			if (hasCulture) {
				variant.state = ContentStateModel.DRAFT;
			}
		});

		this.#documentDb.detail.update(id, document);
	}
}
