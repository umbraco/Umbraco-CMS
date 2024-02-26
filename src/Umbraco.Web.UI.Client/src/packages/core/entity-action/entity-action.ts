import type { UmbAction } from '../action/index.js';
import { UmbActionBase } from '../action/index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string;
	execute(): Promise<void>;
}

export abstract class UmbEntityActionBase<RepositoryType>
	extends UmbActionBase<RepositoryType>
	implements UmbEntityAction<RepositoryType>
{
	entityType: string;
	unique: string;
	repositoryAlias: string;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias);
		this.entityType = entityType;
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}

	abstract execute(): Promise<void>;
}
