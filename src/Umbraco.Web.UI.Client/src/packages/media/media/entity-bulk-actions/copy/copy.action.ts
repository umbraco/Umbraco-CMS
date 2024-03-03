import type { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCopyEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaDetailRepository> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async execute() {
		console.log(`execute bulk copy');
	}
}
