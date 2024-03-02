import { UMB_WORKSPACE_CONTEXT } from './workspace-context.token.js';
import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';

export abstract class UmbEditableWorkspaceContextBase<WorkspaceDataModelType>
	extends UmbContextBase<UmbEditableWorkspaceContextBase<WorkspaceDataModelType>>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly workspaceAlias: string;

	// TODO: We could make a base type for workspace modal data, and use this here: As well as a base for the result, to make sure we always include the unique (instead of the object type)
	public readonly modalContext?: UmbModalContext<{ preset: object }>;

	#isNew = new UmbBooleanState(undefined);
	isNew = this.#isNew.asObservable();

	/*
		Concept notes: [NL]
		Considerations are, if we bring a dirty state (observable) we need to maintain it all the time.
		This might be too heavy process, so we might want to consider just having a get dirty state method.
	*/
	//#isDirty = new UmbBooleanState(undefined);
	//isDirty = this.#isNew.asObservable();

	constructor(host: UmbControllerHost, workspaceAlias: string) {
		super(host, UMB_WORKSPACE_CONTEXT.toString());
		this.workspaceAlias = workspaceAlias;
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			(this.modalContext as UmbModalContext) = context;
		});
	}

	protected resetState() {
		this.#isNew.setValue(undefined);
	}

	getIsNew() {
		return this.#isNew.getValue();
	}

	protected setIsNew(isNew: boolean) {
		this.#isNew.setValue(isNew);
	}

	protected workspaceComplete(data: WorkspaceDataModelType | undefined) {
		if (this.modalContext) {
			if (data) {
				this.modalContext?.setValue(data);
			}
			this.modalContext?.submit();
		}
	}

	//abstract getIsDirty(): Promise<boolean>;
	abstract getUnique(): string | undefined; // TODO: Consider if this should go away/be renamed? now that we have getUnique()
	abstract getEntityType(): string;
	abstract getData(): WorkspaceDataModelType | undefined;
	abstract save(): Promise<void>;
}
