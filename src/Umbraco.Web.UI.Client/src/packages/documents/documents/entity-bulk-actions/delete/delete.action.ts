import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDocumentDeleteEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		console.log('execute bulk delete');
	}
}
