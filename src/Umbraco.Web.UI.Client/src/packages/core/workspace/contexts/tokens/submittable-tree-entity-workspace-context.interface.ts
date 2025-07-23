import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbSubmittableTreeEntityWorkspaceContext extends UmbSubmittableWorkspaceContext {
	/**
	 * The parent entity that the new entity will be created under.
	 * Internal property. Use UMB_PARENT_ENTITY_CONTEXT to get the parent entity.
	 * @type {(Observable<UmbEntityModel | undefined>)}
	 */
	_internal_createUnderParent: Observable<UmbEntityModel | undefined>;

	/**
	 * The entity type that the new entity will be created under.
	 * Internal property. Use UMB_PARENT_ENTITY_CONTEXT to get the parent entity.
	 * @type {(Observable<string | undefined>)}
	 */
	_internal_createUnderParentEntityType: Observable<string | undefined>;

	/**
	 * The entity unique that the new entity will be created under.
	 * Internal property. Use UMB_PARENT_ENTITY_CONTEXT to get the parent entity.
	 * @type {(Observable<UmbEntityUnique | undefined>)}
	 */
	_internal_createUnderParentEntityUnique: Observable<UmbEntityUnique | undefined>;

	/**
	 * The entity type that the new entity will be created under.
	 * Internal property. Use UMB_PARENT_ENTITY_CONTEXT to get the parent entity.
	 * @type {(Observable<string | undefined>)}
	 */
	_internal_getCreateUnderParent(): UmbEntityModel | undefined;

	/**
	 * Sets the parent entity that the new entity will be created under.
	 * Internal property. Use UMB_PARENT_ENTITY_CONTEXT to get the parent entity.
	 * @param {UmbEntityModel | undefined} parent - The parent entity
	 */
	_internal_setCreateUnderParent(parent: UmbEntityModel | undefined): void;

	// TODO: This should be moved to the entity workspace context. It is added here to avoid a breaking change.
	entityType: Observable<string | undefined>;
}
