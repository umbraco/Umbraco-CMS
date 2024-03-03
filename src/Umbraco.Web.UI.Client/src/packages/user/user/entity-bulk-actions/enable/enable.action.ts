import type { UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEnableUserEntityBulkAction extends UmbEntityBulkActionBase<UmbEnableUserRepository> {
	constructor(host: UmbControllerHost, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		await this.repository?.enable(this.selection);
	}
}
