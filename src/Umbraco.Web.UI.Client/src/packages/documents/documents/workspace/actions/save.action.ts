import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import type UmbDocumentWorkspaceContext from '../document-workspace.context.js';
import type { UmbDocumentVariantModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UmbSaveWorkspaceAction,
	type MetaWorkspaceAction,
	type UmbSaveWorkspaceActionArgs,
	type UmbWorkspaceActionDefaultKind,
} from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveWorkspaceAction
	extends UmbSaveWorkspaceAction<MetaWorkspaceAction, UmbDocumentWorkspaceContext>
	implements UmbWorkspaceActionDefaultKind<MetaWorkspaceAction>
{
	#variants: Array<UmbDocumentVariantModel> | undefined;

	constructor(
		host: UmbControllerHost,
		args: UmbSaveWorkspaceActionArgs<MetaWorkspaceAction, UmbDocumentWorkspaceContext>,
	) {
		super(host, { workspaceContextToken: UMB_DOCUMENT_WORKSPACE_CONTEXT, ...args });
	}

	async hasAdditionalOptions() {
		await this._retrieveWorkspaceContext;
		const variantOptions = await this.observe(this._workspaceContext!.variantOptions)
			.asPromise()
			.catch(() => undefined);
		const cultureVariantOptions = variantOptions?.filter((option) => option.culture);
		return cultureVariantOptions ? cultureVariantOptions?.length > 1 : false;
	}

	protected override _gotWorkspaceContext() {
		super._gotWorkspaceContext();
		this.#observeVariants();
		this.#observeReadOnlyGuardRules();
	}

	#observeVariants() {
		this.observe(
			this._workspaceContext?.variants,
			(variants) => {
				this.#variants = variants;
				this.#checkReadOnlyGuardRules();
			},
			'saveWorkspaceActionVariantsObserver',
		);
	}

	#observeReadOnlyGuardRules() {
		this.observe(
			this._workspaceContext?.readOnlyGuard.rules,
			() => this.#checkReadOnlyGuardRules(),
			'umbObserveReadOnlyGuardRules',
		);
	}

	#checkReadOnlyGuardRules() {
		const allVariantsAreReadOnly =
			this.#variants?.filter((variant) =>
				this._workspaceContext!.readOnlyGuard.getIsPermittedForVariant(UmbVariantId.Create(variant)),
			).length === this.#variants?.length;
		if (allVariantsAreReadOnly) {
			this.disable();
		} else {
			this.enable();
		}
	}
}

export { UmbDocumentSaveWorkspaceAction as api };
