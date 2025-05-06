import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbSubmittableTreeEntityWorkspaceContext extends UmbSubmittableWorkspaceContext {
	createUnderParent: Observable<UmbEntityModel | undefined>;
	createUnderParentEntityType: Observable<string | undefined>;
	createUnderParentEntityUnique: Observable<UmbEntityUnique | undefined>;
	getCreateUnderParent(): UmbEntityModel | undefined;
	setCreateUnderParent(parent: UmbEntityModel | undefined): void;

	// TODO: This should be moved to the entity workspace context. It is added here to avoid a breaking change.
	entityType: Observable<string | undefined>;
}
