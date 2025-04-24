import { UMB_VARIANT_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbNumberState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import type { UmbValidationController } from '@umbraco-cms/backoffice/validation';

export class UmbWorkspaceSplitViewContext extends UmbContextBase {
	//
	#variantVariantValidationContext?: UmbValidationController;
	#workspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT.TYPE;
	public getWorkspaceContext() {
		return this.#workspaceContext;
	}

	#datasetContext?: UmbPropertyDatasetContext;

	#index = new UmbNumberState(undefined);
	index = this.#index.asObservable();

	//#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	//variantId = this.#variantId.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_SPLIT_VIEW_CONTEXT);

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._observeVariant();
		});

		this.observe(this.index, () => {
			this._observeVariant();
		});
	}

	private _observeVariant() {
		if (!this.#workspaceContext) return;

		const index = this.#index.getValue();
		if (index === undefined) return;

		this.observe(
			this.#workspaceContext.splitView.activeVariantByIndex(index),
			async (activeVariantInfo) => {
				if (this.#variantVariantValidationContext) {
					this.#variantVariantValidationContext.unprovide();
				}
				if (!activeVariantInfo) return;

				this.#datasetContext?.destroy();
				const variantId = UmbVariantId.Create(activeVariantInfo);

				const validationContext = this.#workspaceContext?.getVariantValidationContext(variantId);
				if (validationContext) {
					validationContext.provideAt(this);
					this.#variantVariantValidationContext = validationContext;
				}

				this.#datasetContext = this.#workspaceContext?.createPropertyDatasetContext(this, variantId);
				this.getHostElement().setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'workspace-split-view:' + variantId.toString());
			},
			'_observeActiveVariant',
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
		this.#index.setValue(index);
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
	'UmbWorkspaceSplitViewContext',
);
