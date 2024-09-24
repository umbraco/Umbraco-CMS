import type { UmbController } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbWorkspaceDataManager<ModelType extends UmbEntityModel> extends UmbController {
	getPersistedData(): ModelType;
	getCurrentData(): ModelType;
}
