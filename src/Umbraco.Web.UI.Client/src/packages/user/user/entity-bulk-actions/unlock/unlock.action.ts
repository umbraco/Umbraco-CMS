import { UmbUnlockUserRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbUnlockUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const repository = new UmbUnlockUserRepository(this._host);
		await repository.unlock(this.selection);
	}
}
