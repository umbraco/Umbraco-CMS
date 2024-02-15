import type { UmbDocumentDetailRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentUnpublishEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentDetailRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute unpublish for: ${this.selection}`);
		//await this.repository?.unpublish();
	}
}
