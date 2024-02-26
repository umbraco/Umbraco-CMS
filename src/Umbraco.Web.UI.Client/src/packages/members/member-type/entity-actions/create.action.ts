import type { UmbMemberTypeDetailRepository } from '../repository/detail/index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbCreateMemberTypeEntityAction extends UmbEntityActionBase<UmbMemberTypeDetailRepository> {
	// TODO: Could EntityActions take the manifest instead, for more flexibility?
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
	}

	async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(null, '', `section/settings/workspace/member-type/create`);
	}
}
