import type { UmbDocumentVariantModel } from '../../types.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import type UmbDocumentWorkspaceContext from '../document-workspace.context.js';
import type { UmbVariantState } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UmbSubmitWorkspaceAction,
	type MetaWorkspaceAction,
	type UmbSubmitWorkspaceActionArgs,
	type UmbWorkspaceActionDefaultKind,
} from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveWorkspaceAction
	extends UmbSubmitWorkspaceAction<MetaWorkspaceAction, UmbDocumentWorkspaceContext>
	implements UmbWorkspaceActionDefaultKind<MetaWorkspaceAction>
{
	#variants: Array<UmbDocumentVariantModel> = [];
	#readOnlyStates: Array<UmbVariantState> = [];

	constructor(host: UmbControllerHost, args: UmbSubmitWorkspaceActionArgs<MetaWorkspaceAction>) {
		super(host, { workspaceContextToken: UMB_DOCUMENT_WORKSPACE_CONTEXT, ...args });
	}

	async hasAdditionalOptions() {
		await this._init;
		const variantOptions = await this.observe(this._workspaceContext!.variantOptions).asPromise();
		return variantOptions?.length > 1;
	}

	override _gotWorkspaceContext() {
		super._gotWorkspaceContext();
		this.#observeVariants();
		this.#observeReadOnlyStates();
	}

	#observeVariants() {
		this.observe(
			this._workspaceContext?.variants,
			(variants) => {
				this.#variants = variants ?? [];
				this.#check();
			},
			'saveWorkspaceActionVariantsObserver',
		);
	}

	#observeReadOnlyStates() {
		this.observe(
			this._workspaceContext?.readOnlyState.states,
			(readOnlyStates) => {
				this.#readOnlyStates = readOnlyStates ?? [];
				this.#check();
			},
			'saveWorkspaceActionReadOnlyStatesObserver',
		);
	}

	#check() {
		const allVariantsAreReadOnly = this.#variants.every((variant) => {
			const variantId = new UmbVariantId(variant.culture, variant.segment);
			return this.#readOnlyStates.some((state) => state.variantId.equal(variantId));
		});

		return allVariantsAreReadOnly ? this.disable() : this.enable();
	}
}

export { UmbDocumentSaveWorkspaceAction as api };
