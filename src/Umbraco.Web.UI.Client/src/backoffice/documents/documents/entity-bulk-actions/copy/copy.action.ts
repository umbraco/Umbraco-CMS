import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbActionBase } from '../../../../shared/action';
import { UmbExecutedEvent } from '../../../../../core/events';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentCopyEntityBulkAction extends UmbActionBase<UmbDocumentRepository> {
	#selection: Array<string>;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.#selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.#selection = selection;
	}

	async execute() {
		console.log(`execute copy for: ${this.#selection}`);
		await this.repository?.copy();
		this.host.dispatchEvent(new UmbExecutedEvent());
	}
}
