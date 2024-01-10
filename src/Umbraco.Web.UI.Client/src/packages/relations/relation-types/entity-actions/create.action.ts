import { UmbRelationTypeRepository } from '../repository/relation-type.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateRelationTypeEntityAction extends UmbEntityActionBase<UmbRelationTypeRepository> {
	// TODO: Could EntityActions take the manifest instead, for more flexibility?
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(null, '', `section/settings/workspace/relation-type/create/${this.unique ?? null}`);
	}
}
