import { UmbEntityWorkspaceContextInterface } from './workspace-entity-context.interface';
import { UmbContextProviderController, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

/*

TODO: We need to figure out if we like to keep using same alias for all workspace contexts.
If so we need to align on a interface that all of these implements. otherwise consumers cant trust the workspace-context.
*/
export abstract class UmbWorkspaceContext<T, EntityType extends UmbEntityBase>
	implements UmbEntityWorkspaceContextInterface<EntityType>
{
	public host: UmbControllerHostElement;
	public repository: T;

	#isNew = new UmbBooleanState(undefined);
	isNew = this.#isNew.asObservable();

	constructor(host: UmbControllerHostElement, repository: T) {
		this.host = host;
		this.repository = repository;
		new UmbContextProviderController(host, UMB_ENTITY_WORKSPACE_CONTEXT, this);
	}

	getIsNew() {
		return this.#isNew.getValue();
	}

	setIsNew(isNew: boolean) {
		this.#isNew.next(isNew);
	}

	abstract getEntityId(): string | undefined; // COnsider if this should go away now that we have getUnique()
	abstract getEntityType(): string; // TODO: consider of this should be on the repository because a repo is responsible for one entity type
	abstract getData(): EntityType | undefined;
	abstract save(): Promise<void>;
	abstract destroy(): void;
}
