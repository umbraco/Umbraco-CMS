import { UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT } from '../global-contexts/index.js';
import type { UmbDocumentPublishingRepository } from '../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<UmbDocumentPublishingRepository> {
	#variantManagerContext?: typeof UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT, (context) => {
			this.#variantManagerContext = context;
		});
	}

	async execute() {
		if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');
		await this.#variantManagerContext.publish(this.unique);
	}
}
