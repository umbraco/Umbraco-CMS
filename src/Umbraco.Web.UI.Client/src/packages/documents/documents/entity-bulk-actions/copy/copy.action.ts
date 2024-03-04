import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDocumentCopyEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		console.log('execute bulk copy');
	}
}
