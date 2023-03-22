import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export class UmbDocumentCopyEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute copy for: ${this.selection}`);
		await this.repository?.copy();
	}
}
