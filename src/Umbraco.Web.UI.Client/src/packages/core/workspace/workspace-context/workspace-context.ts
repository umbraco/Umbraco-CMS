import { UmbEntityWorkspaceContextInterface } from './workspace-entity-context.interface.js';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '@umbraco-cms/backoffice/modal';

/*

TODO: We need to figure out if we like to keep using same alias for all workspace contexts.
If so we need to align on a interface that all of these implements. otherwise consumers cant trust the workspace-context.
*/
export abstract class UmbWorkspaceContext<RepositoryType, EntityType extends UmbEntityBase>
	implements UmbEntityWorkspaceContextInterface<EntityType>
{
	public readonly host: UmbControllerHostElement;
	public readonly workspaceAlias: string;
	public readonly repository: RepositoryType;

	// TODO: We could make a base type for workspace modal data, and use this here: As well as a base for the result, to make sure we always include the unique.
	public readonly modalContext?: UmbModalContext<{ preset: object }>;

	#isNew = new UmbBooleanState(undefined);
	isNew = this.#isNew.asObservable();

	constructor(host: UmbControllerHostElement, workspaceAlias: string, repository: RepositoryType) {
		this.host = host;
		this.workspaceAlias = workspaceAlias;
		this.repository = repository;
		new UmbContextProviderController(host, UMB_WORKSPACE_CONTEXT, this);
		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (context) => {
			(this.modalContext as UmbModalContext) = context;
		});
	}

	getIsNew() {
		return this.#isNew.getValue();
	}

	setIsNew(isNew: boolean) {
		this.#isNew.next(isNew);
	}

	protected saveComplete(data: EntityType) {
		if (this.modalContext) {
			this.submitModal(data);
		} else {
			// No need to have UI changing to "not new" if we are in a modal.
			this.setIsNew(false);
		}
	}

	protected submitModal(data: EntityType) {
		this.modalContext?.submit(data);
	}

	abstract getEntityId(): string | undefined; // COnsider if this should go away now that we have getUnique()
	abstract getEntityType(): string; // TODO: consider of this should be on the repository because a repo is responsible for one entity type
	abstract getData(): EntityType | undefined;
	abstract save(): Promise<void>;
	abstract destroy(): void;
}
