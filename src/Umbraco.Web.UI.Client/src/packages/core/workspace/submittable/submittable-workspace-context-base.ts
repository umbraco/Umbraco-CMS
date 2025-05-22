import { UmbWorkspaceRouteManager } from '../controllers/workspace-route-manager.controller.js';
import { UMB_WORKSPACE_CONTEXT } from '../workspace.context-token.js';
import type { UmbSubmittableWorkspaceContext } from '../contexts/tokens/submittable-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbValidationController } from '@umbraco-cms/backoffice/validation';

export abstract class UmbSubmittableWorkspaceContextBase<WorkspaceDataModelType>
	extends UmbContextBase
	implements UmbSubmittableWorkspaceContext
{
	public readonly workspaceAlias: string;

	// TODO: We could make a base type for workspace modal data, and use this here: As well as a base for the result, to make sure we always include the unique (instead of the object type)
	public readonly modalContext?: UmbModalContext<{ preset: object }>;

	#validationContexts: Array<UmbValidationController> = [];

	/**
	 * Appends a validation context to the workspace.
	 * @param context
	 */
	addValidationContext(context: UmbValidationController) {
		this.#validationContexts.push(context);
	}

	#submitPromise: Promise<void> | undefined;
	#submitResolve: (() => void) | undefined;
	#submitReject: ((reason?: any) => void) | undefined;

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
		// TODO: Consider if we can move this consumption to #resolveSubmit, just as a getContext, but it depends if others use the modalContext prop.. [NL]
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			(this.modalContext as UmbModalContext | undefined) = context;
		});
	}

	protected resetState() {
		this.#validationContexts.forEach((context) => context.reset());
		this.#isNew.setValue(undefined);
	}

	public getIsNew() {
		return this.#isNew.getValue();
	}

	protected setIsNew(isNew: boolean) {
		this.#isNew.setValue(isNew);
	}

	/**
	 * If a Workspace has multiple validation contexts, then this method can be overwritten to return the correct one.
	 * @returns Promise that resolves to void when the validation is complete.
	 */
	public async validate(): Promise<Array<void>> {
		return await Promise.all(this.#validationContexts.map((context) => context.validate()));
	}

	public async requestSubmit(): Promise<void> {
		return this.validateAndSubmit(
			() => this.submit(),
			(reason?: any) => this.invalidSubmit(reason),
		);
	}

	protected async _validateAndLog(): Promise<void> {
		await this.validate().catch(async () => {
			// TODO: Implement developer-mode logging here. [NL]
			console.warn(
				'Validation failed because of these validation messages still begin present: ',
				this.#validationContexts.flatMap((x) => x.messages.getMessages()),
			);
			return Promise.reject();
		});
	}

	public async validateAndSubmit(
		onValid: () => Promise<void>,
		onInvalid: (reason?: any) => Promise<void>,
	): Promise<void> {
		if (this.#submitPromise) {
			return this.#submitPromise;
		}
		this.#submitPromise = new Promise<void>((resolve, reject) => {
			this.#submitResolve = resolve;
			this.#submitReject = reject;
		});
		this._validateAndLog().then(
			async () => {
				onValid().then(this.#completeSubmit, this.#rejectSubmit);
			},
			async (error) => {
				onInvalid(error).then(this.#resolveSubmit, this.#rejectSubmit);
			},
		);

		return await this.#submitPromise;
	}

	#rejectSubmit = (error: any) => {
		if (this.#submitPromise) {
			// TODO: Capture the validation contexts messages on open, and then reset to them in this case. [NL]

			this.#submitReject?.(error);
			this.#submitPromise = undefined;
			this.#submitResolve = undefined;
			this.#submitReject = undefined;
		}
	};

	#resolveSubmit = () => {
		// Resolve the submit promise:
		this.#submitResolve?.();
		this.#submitPromise = undefined;
		this.#submitResolve = undefined;
		this.#submitReject = undefined;
	};

	#completeSubmit = () => {
		this.#resolveSubmit();

		// Calling reset on the validation context here. [NL]
		// TODO: Capture the validation messages on open, and then reset to that.
		//this.validation.reset();

		this._closeModal();
	};

	protected _closeModal() {
		if (this.modalContext) {
			this.modalContext?.setValue(this.getData());
			this.modalContext?.submit();
		}
	}

	//abstract getIsDirty(): Promise<boolean>;
	abstract getUnique(): string | null | undefined;
	abstract getEntityType(): string;
	abstract getData(): WorkspaceDataModelType | undefined;
	protected abstract submit(): Promise<void>;
	protected invalidSubmit(reason?: any): Promise<void> {
		return Promise.reject(reason);
	}
}

/*
 * @deprecated Use UmbSubmittableWorkspaceContextBase instead — Will be removed before RC.
 * Rename `save` to `submit` and return a promise that resolves to true when save is complete.
 * TODO: Delete before RC.
 */
export abstract class UmbEditableWorkspaceContextBase<
	WorkspaceDataModelType,
> extends UmbSubmittableWorkspaceContextBase<WorkspaceDataModelType> {}
