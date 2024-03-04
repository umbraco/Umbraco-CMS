import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDocumentDuplicateEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		console.log('execute bulk duplicate');
	}
}
