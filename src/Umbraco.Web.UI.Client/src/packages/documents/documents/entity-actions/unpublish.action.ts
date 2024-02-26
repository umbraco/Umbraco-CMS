import type { UmbDocumentPublishingRepository } from '../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbUnpublishDocumentEntityAction extends UmbEntityActionBase<UmbDocumentPublishingRepository> {
	async execute() {
		throw new Error('This action not implemented.');
		//if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');
		//await this.#variantManagerContext.unpublish(this.unique);
	}
}
