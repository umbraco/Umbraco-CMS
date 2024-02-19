import type { UmbAction } from '../action/index.js';
import { UmbActionBase } from '../action/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string | null;
}

export class UmbEntityActionBase<RepositoryType> extends UmbActionBase<RepositoryType> {
	entityType: string;
	unique: string | null;
	repositoryAlias: string;

	constructor(host: UmbControllerHost, repositoryAlias: string, unique: string | null, entityType: string) {
		super(host, repositoryAlias);
		this.entityType = entityType;
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}
}
