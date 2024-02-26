import type { UmbDocumentDetailRepository } from '../../repository/index.js';
import { UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT } from '../../global-contexts/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentDetailRepository> {
	#variantManagerContext?: typeof UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		this.consumeContext(UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT, (context) => {
			this.#variantManagerContext = context;
		});
	}

	async execute() {
		if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');
		await this.#variantManagerContext.unpublishIndescriminate(this.selection);
	}
}
