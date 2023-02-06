import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentPermissionsEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		await this.repository?.setPermissions();
	}
}
