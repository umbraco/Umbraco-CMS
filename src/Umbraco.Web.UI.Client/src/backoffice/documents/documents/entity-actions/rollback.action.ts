import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/entity-actions';
import { UmbExecutedEvent } from '../../../../core/events';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbRollbackDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.rollback();
		this.host.dispatchEvent(new UmbExecutedEvent());
	}
}
