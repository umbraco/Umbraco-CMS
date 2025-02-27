import type { UmbDocumentVariantModel } from '../../types.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import type UmbDocumentWorkspaceContext from '../document-workspace.context.js';
import type { UmbVariantState } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentSaveWorkspaceAction extends UmbSubmitWorkspaceAction {
	#documentWorkspaceContext?: UmbDocumentWorkspaceContext;
	#variants: Array<UmbDocumentVariantModel> = [];
	#readOnlyStates: Array<UmbVariantState> = [];

	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#documentWorkspaceContext = context;
			this.#observeVariants();
			this.#observeReadOnlyStates();
		});
	}

	#observeVariants() {
		this.observe(
			this.#documentWorkspaceContext?.variants,
			(variants) => {
				this.#variants = variants ?? [];
				this.#check();
			},
			'saveWorkspaceActionVariantsObserver',
		);
	}

	#observeReadOnlyStates() {
		this.observe(
			this.#documentWorkspaceContext?.readOnlyState.states,
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
