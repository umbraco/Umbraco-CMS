import type { UmbUnlockUserRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUnlockUserEntityBulkAction extends UmbEntityBulkActionBase<UmbUnlockUserRepository> {
	constructor(host: UmbControllerHost, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		await this.repository?.unlock(this.selection);
	}
}
