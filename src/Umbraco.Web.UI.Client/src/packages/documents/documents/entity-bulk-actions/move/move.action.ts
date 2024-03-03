import type { UmbDocumentDetailRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentDetailRepository> {
	constructor(host: UmbControllerHost, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute move for: ${this.selection}`);
		//await this.repository?.move();
	}
}
