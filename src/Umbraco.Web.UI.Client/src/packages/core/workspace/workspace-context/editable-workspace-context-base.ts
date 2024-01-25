import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';

export abstract class UmbEditableWorkspaceContextBase<WorkspaceData>
	extends UmbBaseController
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly workspaceAlias: string;

	// TODO: We could make a base type for workspace modal data, and use this here: As well as a base for the result, to make sure we always include the unique (instead of the object type)
	public readonly modalContext?: UmbModalContext<{ preset: object }>;

	#isNew = new UmbBooleanState(undefined);
	isNew = this.#isNew.asObservable();

	constructor(host: UmbControllerHost, workspaceAlias: string) {
		super(host);
		this.workspaceAlias = workspaceAlias;
		this.provideContext(UMB_WORKSPACE_CONTEXT, this);
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			(this.modalContext as UmbModalContext) = context;
		});
	}

	getIsNew() {
		return this.#isNew.getValue();
	}

	setIsNew(isNew: boolean) {
		this.#isNew.setValue(isNew);
	}

	protected saveComplete(data: WorkspaceData) {
		if (this.modalContext) {
			this.modalContext?.setValue(data);
			this.modalContext?.submit();
		} else {
			// No need to have UI changing to "not new" if we are in a modal.
			this.setIsNew(false);
		}
	}

	abstract getEntityId(): string | undefined; // TODO: Consider if this should go away/be renamed? now that we have getUnique()
	abstract getEntityType(): string; // TODO: consider if this should be on the repository because a repo is responsible for one entity type
	abstract getData(): WorkspaceData | undefined;
	abstract save(): Promise<void>;
}
