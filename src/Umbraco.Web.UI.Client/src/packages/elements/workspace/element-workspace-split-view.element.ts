import { UMB_ELEMENT_WORKSPACE_ALIAS, UMB_ELEMENT_WORKSPACE_CONTEXT } from './constants.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UmbActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './element-workspace-split-view-variant-selector.element.js';

@customElement('umb-element-workspace-split-view')
export class UmbElementWorkspaceSplitViewElement extends UmbLitElement {
	// TODO: Refactor: use the split view context token:
	private _workspaceContext?: typeof UMB_ELEMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _variants?: Array<UmbActiveVariant>;

	@state()
	private _icon?: string;

	@state()
	private _loading = true;

	constructor() {
		super();
		// TODO: Refactor: use a split view workspace context token:
		this.consumeContext(UMB_ELEMENT_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.#observeActiveVariantInfo();
			this.#observeIcon();
			this.#observeLoading();
		});
	}

	#observeActiveVariantInfo() {
		if (!this._workspaceContext) return;
		this.observe(
			this._workspaceContext.splitView.activeVariantsInfo,
			(variants) => {
				this._variants = variants;
			},
			'_observeActiveVariantsInfo',
		);
	}

	#observeIcon() {
		this.observe(
			this._workspaceContext?.contentTypeIcon,
			(icon) => {
				this._icon = icon ?? undefined;
			},
			'observeIcon',
		);
	}

	#observeLoading() {
		this.observe(
			this._workspaceContext?.loading.isOn,
			(loading) => {
				this._loading = loading ?? false;
			},
			'observeLoading',
		);
	}

	override render() {
		return this._variants
			? html`<div id="splitViews">
						${repeat(
							this._variants,
							(view) =>
								view.index + '_' + (view.culture ?? '') + '_' + (view.segment ?? '') + '_' + this._variants!.length,
							(view) => html`
								<umb-workspace-split-view
									.loading=${this._loading}
									.displayNavigation=${view.index === this._variants!.length - 1}
									.splitViewIndex=${view.index}>
									<umb-icon slot="icon" name=${ifDefined(this._icon)}></umb-icon>
									<umb-element-workspace-split-view-variant-selector
										slot="variant-selector"></umb-element-workspace-split-view-variant-selector>
								</umb-workspace-split-view>
							`,
						)}
					</div>

					<umb-workspace-footer alias=${UMB_ELEMENT_WORKSPACE_ALIAS}></umb-workspace-footer>`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				width: 100%;
				height: 100%;

				display: flex;
				flex: 1;
				flex-direction: column;
			}

			#splitViews {
				display: flex;
				width: 100%;
				height: calc(100% - var(--umb-footer-layout-height));
			}
		`,
	];
}

export default UmbElementWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-split-view': UmbElementWorkspaceSplitViewElement;
	}
}
