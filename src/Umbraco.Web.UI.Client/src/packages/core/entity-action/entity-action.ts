import type { UmbAction } from '../action/index.js';
import { UmbActionBase } from '../action/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string;
	getHref(): Promise<string | null | undefined>;
	execute(): Promise<void>;
}

export abstract class UmbEntityActionBase<RepositoryType>
	extends UmbActionBase<RepositoryType>
	implements UmbEntityAction<RepositoryType>
{
	entityType: string;
	unique: string;
	repositoryAlias: string;

	constructor(host: UmbControllerHost, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias);
		this.entityType = entityType;
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}

	public getHref(): Promise<string | null | undefined> {
		return Promise.resolve(undefined);
	}
	public execute(): Promise<void> {
		return Promise.resolve();
	}
}
