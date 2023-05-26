import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbAction<RepositoryType = unknown> {
	host: UmbControllerHostElement;
	repository: RepositoryType;
	execute(): Promise<void>;
}
