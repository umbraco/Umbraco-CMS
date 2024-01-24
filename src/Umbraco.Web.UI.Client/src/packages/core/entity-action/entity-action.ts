import { UmbAction, UmbActionBase } from '../action/index.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string;
}

export class UmbEntityActionBase<RepositoryType> extends UmbActionBase<RepositoryType> {
	entityType: string;
	unique: string;
	repositoryAlias: string;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias);
		this.entityType = entityType;
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}
}
