import { UmbDocumentRepository } from '../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`publish: ${this.unique}`);
		// TODO: implement dialog or something to handle variants.
		//await this.repository?.publish();
	}
}
