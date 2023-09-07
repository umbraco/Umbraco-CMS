import { UmbVariantContext } from '../variant-context/index.js';
import { UMB_VARIANT_WORKSPACE_CONTEXT_TOKEN } from '../index.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbNumberState,
} from '@umbraco-cms/backoffice/observable-api';


export class UmbWorkspaceSplitViewContext extends UmbBaseController {

	#workspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT_TOKEN.TYPE;
	public getWorkspaceContext() {
		return this.#workspaceContext;
	}

	#variantContext?: UmbVariantContext;

	#index = new UmbNumberState(undefined);
	index = this.#index.asObservable();

	//#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	//variantId = this.#variantId.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT_TOKEN, (context) => {
			this.#workspaceContext = context;
			this._observeVariant();
		});

		this.observe(this.#index, () => {
			this._observeVariant();
		});


		this.provideContext(UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, this);
	}

	private _observeVariant() {
		if (!this.#workspaceContext) return;

		const index = this.#index.getValue();
		if (index === undefined) return;

		// TODO: Should splitView be put into its own context?... a split view manager context?   one which might have a reference to the workspace context, so we still can ask that about how to create the variant context.
		this.observe(
			this.#workspaceContext.splitView.activeVariantByIndex(index),
			async (activeVariantInfo) => {
				if (!activeVariantInfo) return;

				// TODO: Ask workspace context to create the specific variant context.

				this.#variantContext?.destroy();
				const variantId = UmbVariantId.Create(activeVariantInfo);
				this.#variantContext = this.#workspaceContext?.createVariantContext(this, variantId);
			},
			'_observeActiveVariant'
		);
	}


	public switchVariant(variant: UmbVariantId) {
		const index = this.#index.value;
		if (index === undefined) return;
		this.#workspaceContext?.splitView.switchVariant(index, variant);
	}

	public closeSplitView() {
		const index = this.#index.value;
		if (index === undefined) return;
		this.#workspaceContext?.splitView.closeSplitView(index);
	}

	public openSplitView(variant: UmbVariantId) {
		this.#workspaceContext?.splitView.openSplitView(variant);
	}

	public getSplitViewIndex() {
		return this.#index.getValue();
	}
	public setSplitViewIndex(index: number) {
		this.#index.next(index);
	}




	/**
	 *
	 * concept this class could have methods to set and get the culture and segment of the active variant? just by using the index.
	 */

	/*
	public destroy(): void {

	}
	*/
}

export const UMB_WORKSPACE_SPLIT_VIEW_CONTEXT = new UmbContextToken<UmbWorkspaceSplitViewContext>(
	'umbWorkspaceSplitViewContext'
);
