import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbActionBase } from '../../../shared/components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbPublishDocumentEntityAction extends UmbActionBase<UmbDocumentRepository> {
	#unique: string;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.#unique = unique;
	}

	async execute() {
		console.log(`execute for: ${this.#unique}`);
		await this.repository?.publish();
	}
}
