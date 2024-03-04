import { UmbDisableUserRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDisableUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const repository = new UmbDisableUserRepository(this._host);
		await repository.disable(this.selection);
	}
}
