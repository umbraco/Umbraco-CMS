import { UmbEnableUserRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbEnableUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const repository = new UmbEnableUserRepository(this._host);
		await repository.enable(this.selection);
	}
}
