import { UmbEntityBulkActionBase } from '@umbraco-cms/entity-action';
import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbMediaCopyEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute copy for: ${this.selection}`);
		await this.repository?.copy([], '');
	}
}
