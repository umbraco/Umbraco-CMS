import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/components';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPermissionsEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.setPermissions();
	}
}
