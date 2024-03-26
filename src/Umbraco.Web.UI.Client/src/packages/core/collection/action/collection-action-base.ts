import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionAction extends UmbApi {
	execute(): Promise<void>;
}

export abstract class UmbCollectionActionBase extends UmbControllerBase implements UmbCollectionAction {
	abstract execute(): Promise<void>;
}
