import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);
	}

	async execute() {
		console.log(`execute move for: ${this.selection}`);
		await this.repository?.move();
	}
}
