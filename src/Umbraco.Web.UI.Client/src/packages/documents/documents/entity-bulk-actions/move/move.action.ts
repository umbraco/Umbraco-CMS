import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbMoveDocumentEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		console.log(`execute bulk move`);
	}
}
