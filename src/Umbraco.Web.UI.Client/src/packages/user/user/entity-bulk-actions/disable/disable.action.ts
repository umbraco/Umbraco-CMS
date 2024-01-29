import type { UmbDisableUserRepository } from '../../repository/disable/disable-user.repository.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbDisableUserEntityBulkAction extends UmbEntityBulkActionBase<UmbDisableUserRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		await this.repository?.disable(this.selection);
	}
}
