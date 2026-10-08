import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../context/document-workspace.context-token.js';
import type UmbDocumentWorkspaceContext from '../context/document-workspace.context.js';
import type { UmbDocumentVariantModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
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
	constructor(
		host: UmbControllerHost,
		args: UmbSaveWorkspaceActionArgs<MetaWorkspaceAction, UmbDocumentWorkspaceContext>,
	) {
		super(host, { workspaceContextToken: UMB_DOCUMENT_WORKSPACE_CONTEXT, ...args });
	}

	async hasAdditionalOptions() {
		await this._retrieveWorkspaceContext;
		if (!this._workspaceContext) return false;
		const variantOptions = await this.observe(this._workspaceContext.variantOptions)
			.asPromise()
			.catch(() => undefined);
		const cultureVariantOptions = variantOptions?.filter((option) => option.culture);
		return cultureVariantOptions ? cultureVariantOptions?.length > 1 : false;
	}

	protected override _gotWorkspaceContext() {
		super._gotWorkspaceContext();
		this.observe(
			this._workspaceContext?.variants,
			(variants) => this.#observeWritableVariants(variants ?? []),
			'saveWorkspaceActionVariantsObserver',
		);
	}

	#observeWritableVariants(variants: Array<UmbDocumentVariantModel>) {
		const workspaceContext = this._workspaceContext;
		if (!workspaceContext) return;

		// The invariant (shared) data is handled as a variant of its own, saved on its own for existing content.
		const variantIds = [
			...variants.map((variant) => UmbVariantId.CreateFromPartial(variant)),
			UmbVariantId.CreateInvariant(),
		];

		this.observe(
			combineLatest(variantIds.map((variantId) => workspaceContext.isWritableVariant(variantId))),
			(writable) => {
				const isNew = workspaceContext.getIsNew();
				const hasWritableVariant = variantIds.some(
					(variantId, index) =>
						writable[index] && (isNew === false || !variantId.equal(UmbVariantId.CreateInvariant())),
				);
				if (hasWritableVariant) {
					this.enable();
				} else {
					this.disable();
				}
			},
			'saveWorkspaceActionWritableVariantsObserver',
		);
	}
}

export { UmbDocumentSaveWorkspaceAction as api };
