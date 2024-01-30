import type { UmbDocumentDetailRepository } from '../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<UmbDocumentDetailRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		//await this.repository?.setPublicAccess();
	}
}
