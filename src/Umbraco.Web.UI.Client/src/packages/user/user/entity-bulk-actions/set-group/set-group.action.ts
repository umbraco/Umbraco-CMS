import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbSetGroupUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		//TODO: Implement
		throw new Error('Bulk set group is not implemented yet');
	}
}

export { UmbSetGroupUserEntityBulkAction as api };
