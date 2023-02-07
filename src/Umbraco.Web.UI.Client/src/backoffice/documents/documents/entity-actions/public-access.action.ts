import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbActionBase } from '../../../shared/entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentPublicAccessEntityAction extends UmbActionBase<UmbDocumentRepository> {
	#unique: string;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.#unique = unique;
	}

	async execute() {
		console.log(`execute for: ${this.#unique}`);
		await this.repository?.setPublicAccess();
	}
}
