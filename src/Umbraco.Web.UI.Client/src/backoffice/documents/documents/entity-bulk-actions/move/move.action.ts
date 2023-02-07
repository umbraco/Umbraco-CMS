import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbActionBase } from '../../../../shared/entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentMoveEntityBulkAction extends UmbActionBase<UmbDocumentRepository> {
	#selection: Array<string>;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.#selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.#selection = selection;
	}

	async execute() {
		console.log(`execute move for: ${this.#selection}`);
		await this.repository?.move();
	}
}
