import { UmbAction, UmbActionBase } from './action';
import { UmbControllerHostElement } from 'src/libs/controller-api';

export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	unique: string;
}

export class UmbEntityActionBase<RepositoryType> extends UmbActionBase<RepositoryType> {
	unique: string;
	repositoryAlias: string;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}
}
