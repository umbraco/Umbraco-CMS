import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbPublishDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, unique: string) {
		super(host, UmbDocumentRepository, unique);
	}

	async execute() {
		await this.repository.publish();
	}
}
