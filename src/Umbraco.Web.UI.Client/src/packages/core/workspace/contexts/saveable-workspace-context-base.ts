import { UmbWorkspaceRouteManager } from '../controllers/workspace-route-manager.controller.js';
//import { UmbValidationContext } from '../../validation/context/validation.context.js';
import { UMB_WORKSPACE_CONTEXT } from './tokens/workspace.context-token.js';
import type { UmbSaveableWorkspaceContext } from './tokens/saveable-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_FORM_CONTEXT } from '@umbraco-cms/backoffice/form';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export abstract class UmbSaveableWorkspaceContextBase<WorkspaceDataModelType>
	extends UmbContextBase<UmbSaveableWorkspaceContextBase<WorkspaceDataModelType>>
	implements UmbSaveableWorkspaceContext
{
	public readonly workspaceAlias: string;

	// TODO: We could make a base type for workspace modal data, and use this here: As well as a base for the result, to make sure we always include the unique (instead of the object type)
	public readonly modalContext?: UmbModalContext<{ preset: object }>;

	//readonly #validation = new UmbValidationContext(this);
	#form?: typeof UMB_FORM_CONTEXT.TYPE;
	#savePromise: Promise<void> | undefined;
	#saveResolve: (() => void) | undefined;

	abstract readonly unique: Observable<string | null | undefined>;

	#isNew = new UmbBooleanState(undefined);
	isNew = this.#isNew.asObservable();

	readonly routes = new UmbWorkspaceRouteManager(this);

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
		this.#performSaveBind = this.submit.bind(this);
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			(this.modalContext as UmbModalContext) = context;
		});
		this.consumeContext(UMB_FORM_CONTEXT, (context) => {
			if (this.#form === context) return;
			if (this.#form) {
				this.#form.removeEventListener('submit', this.#performSaveBind);
			}
			this.#form = context;
			this.#form.addEventListener('submit', this.#performSaveBind);
			this._gotFormContext(context);
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
		// Resolve the save promise:
		this.#saveResolve?.();

		if (this.modalContext) {
			if (data) {
				this.modalContext?.setValue(data);
			}
			this.modalContext?.submit();
		}
	}

	protected _gotFormContext(context: typeof UMB_FORM_CONTEXT.TYPE): void {}

	//abstract getIsDirty(): Promise<boolean>;
	abstract getUnique(): string | undefined;
	abstract getEntityType(): string;
	abstract getData(): WorkspaceDataModelType | undefined;
	save(): Promise<void> {
		if (this.#savePromise) {
			return this.#savePromise;
		}
		if (!this.#form) {
			throw new Error('Form context not available');
		}
		this.#form.requestSubmit();

		this.#savePromise = new Promise<void>((resolve) => {
			this.#saveResolve = resolve;
		});
		return this.#savePromise;
	}

	#performSaveBind: () => void;
	protected abstract submit(): void;
}

/*
 * @deprecated Use UmbSaveableWorkspaceContextBase instead — Will be removed before RC.
 * TODO: Delete before RC.
 */
export abstract class UmbEditableWorkspaceContextBase<
	WorkspaceDataModelType,
> extends UmbSaveableWorkspaceContextBase<WorkspaceDataModelType> {}
