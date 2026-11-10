import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../context/document-workspace.context-token.js';
import type UmbDocumentWorkspaceContext from '../context/document-workspace.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { combineLatest, map, switchMap } from '@umbraco-cms/backoffice/external/rxjs';
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
		const workspaceContext = this._workspaceContext;
		if (!workspaceContext) return;

		this.observe(
			combineLatest([workspaceContext.variants, workspaceContext.variesByCulture, workspaceContext.isNew]).pipe(
				switchMap(([variants, variesByCulture, isNew]) => {
					const variantIds = variants.map((variant) => UmbVariantId.CreateFromPartial(variant));
					// The shared data of culture-varying content can be saved on its own, but cannot create the content.
					if (variesByCulture && isNew === false) {
						variantIds.push(UmbVariantId.CreateInvariant());
					}
					return combineLatest(variantIds.map((variantId) => workspaceContext.isWritableVariant(variantId)));
				}),
				map((writable) => writable.some(Boolean)),
			),
			(canSave) => {
				if (canSave) {
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
