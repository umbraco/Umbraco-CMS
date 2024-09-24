import { UmbSubmittableWorkspaceContextBase } from '../submittable/index.js';
import { UmbEntityWorkspaceDataManager } from './entity-workspace-data-manager.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export abstract class UmbEntityWorkspaceContextBase<
	EntityModelType extends UmbEntityModel,
> extends UmbSubmittableWorkspaceContextBase<EntityModelType> {
	protected readonly _data = new UmbEntityWorkspaceDataManager<EntityModelType>(this);
}
