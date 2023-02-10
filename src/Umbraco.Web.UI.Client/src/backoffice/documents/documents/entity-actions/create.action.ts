import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		alert('open create dialog');
	}
}
