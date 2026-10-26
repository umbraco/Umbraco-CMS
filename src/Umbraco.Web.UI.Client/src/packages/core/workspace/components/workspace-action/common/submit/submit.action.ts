import type { MetaWorkspaceAction } from '../../../../types.js';
import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../../../../contexts/tokens/index.js';
import type { UmbSubmittableWorkspaceContext } from '../../../../contexts/tokens/index.js';
import type { UmbEntityDetailWorkspaceContextBase } from '../../../../entity-detail/entity-detail-workspace-base.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import type { UmbSubmitWorkspaceActionArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSubmitWorkspaceAction<
	ArgsMetaType extends MetaWorkspaceAction = MetaWorkspaceAction,
	WorkspaceContextType extends UmbSubmittableWorkspaceContext = UmbSubmittableWorkspaceContext,
> extends UmbWorkspaceActionBase<ArgsMetaType> {
	protected _retrieveWorkspaceContext?: Promise<unknown>;
	protected _workspaceContext?: WorkspaceContextType;

	#hasUnique = false;
	#isReadOnly = false;

	constructor(host: UmbControllerHost, args: UmbSubmitWorkspaceActionArgs<ArgsMetaType>) {
		super(host, args);

		// TODO: Could we make change label depending on the state? [NL]
		this._retrieveWorkspaceContext = this.consumeContext(
			args.workspaceContextToken ?? UMB_SUBMITTABLE_WORKSPACE_CONTEXT,
			(context) => {
				this._workspaceContext = context as WorkspaceContextType;
				this.#observeSubmittable();
				this._gotWorkspaceContext();
			},
		)
			.asPromise()
			.catch(() => {
				return undefined;
			});
	}

	#observeSubmittable() {
		this.observe(
			this._workspaceContext?.unique,
			(unique) => {
				// We can't save if we don't have a unique
				this.#hasUnique = unique !== undefined;
				this.#updateIsDisabled();
			},
			'saveWorkspaceActionUniqueObserver',
		);

		const entityDetailContext = this.#isEntityDetailWorkspaceContext(this._workspaceContext)
			? this._workspaceContext
			: undefined;
		this.observe(
			entityDetailContext?.readOnly,
			(isReadOnly) => {
				this.#isReadOnly = isReadOnly ?? false;
				this.#updateIsDisabled();
			},
			'saveWorkspaceActionReadOnlyObserver',
		);
	}

	#updateIsDisabled() {
		if (!this._workspaceContext) return;
		if (this.#hasUnique && !this.#isReadOnly) {
			// Dangerous, cause this could enable despite a class extension decided to disable it?. [NL]
			this.enable();
		} else {
			this.disable();
		}
	}

	protected _gotWorkspaceContext() {
		// Override in subclass
	}

	override async execute() {
		await this._retrieveWorkspaceContext;
		return await this._workspaceContext?.requestSubmit();
	}

	#isEntityDetailWorkspaceContext(context: unknown): context is UmbEntityDetailWorkspaceContextBase {
		return (context as UmbEntityDetailWorkspaceContextBase | undefined)?.IS_ENTITY_DETAIL_WORKSPACE_CONTEXT === true;
	}
}
