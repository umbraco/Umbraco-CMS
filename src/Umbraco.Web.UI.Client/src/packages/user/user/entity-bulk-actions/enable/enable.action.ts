import type { UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbEnableUserEntityBulkAction extends UmbEntityBulkActionBase<UmbEnableUserRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		await this.repository?.enable(this.selection);
	}
}
