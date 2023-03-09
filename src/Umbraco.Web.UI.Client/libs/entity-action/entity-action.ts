import { UmbAction, UmbActionBase } from './action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string;
}

export class UmbEntityActionBase<RepositoryType> extends UmbActionBase<RepositoryType> {
	unique: string;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.unique = unique;
	}
}
