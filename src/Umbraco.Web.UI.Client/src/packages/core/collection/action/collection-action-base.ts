import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionAction extends UmbApi {
	execute(): Promise<void>;
}

export abstract class UmbCollectionActionBase extends UmbBaseController implements UmbCollectionAction {
	abstract execute(): Promise<void>;
}
