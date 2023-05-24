import type { UmbMediaRepository } from '../../repository/media.repository.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCopyEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute copy for: ${this.selection}`);
		await this.repository?.copy([], '');
	}
}
