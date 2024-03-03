import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentMoveEntityBulkAction extends UmbEntityBulkActionBase<never> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async execute() {
		console.log(`execute bulk move`);
	}
}
