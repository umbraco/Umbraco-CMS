import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { UmbRepositoryFactory } from '@umbraco-cms/models';

export interface UmbEntityAction<T> {
	unique: string;
	repository: T;
	execute(): Promise<void>;
}

export class UmbEntityActionBase<T> {
	host: UmbControllerHostInterface;
	unique: string;
	repository: T;

	constructor(host: UmbControllerHostInterface, repository: UmbRepositoryFactory<T>, unique: string) {
		this.host = host;
		this.unique = unique;
		this.repository = new repository(this.host);
	}
}
