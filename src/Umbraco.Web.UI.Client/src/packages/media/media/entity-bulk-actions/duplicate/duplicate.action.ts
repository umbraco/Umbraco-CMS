import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDuplicateMediaEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		console.log('execute bulk duplicate');
	}
}

export { UmbDuplicateMediaEntityBulkAction as api };
