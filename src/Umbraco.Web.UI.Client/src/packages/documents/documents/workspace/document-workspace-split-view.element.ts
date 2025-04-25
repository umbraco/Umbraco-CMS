import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './document-workspace-split-view-variant-selector.element.js';

@customElement('umb-document-workspace-split-view')
export class UmbDocumentWorkspaceSplitViewElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_isNew?: boolean;

	@state()
	_variants?: Array<ActiveVariant>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.#observeActiveVariantInfo();
			this.#observeIsNew();
		});
	}

	#observeActiveVariantInfo() {
		this.observe(
			this._workspaceContext?.splitView.activeVariantsInfo,
			(variants) => {
				this._variants = variants;
			},
			'_observeActiveVariantsInfo',
		);
	}

	#observeIsNew() {
		this.observe(
			this._workspaceContext?.isNew,
			(isNew) => {
				this._isNew = isNew;
			},
			'#observeIsNew',
		);
	}

	#getDisplayNavigation(view: ActiveVariant) {
		return view.index === this._variants!.length - 1 && this._isNew == false;
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
									.splitViewIndex=${view.index}
									.displayNavigation=${this.#getDisplayNavigation(view)}>
									<umb-document-workspace-split-view-variant-selector
										slot="variant-selector"></umb-document-workspace-split-view-variant-selector>
								</umb-workspace-split-view>
							`,
						)}
					</div>

					<umb-workspace-footer alias="Umb.Workspace.Document"></umb-workspace-footer>`
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

			#breadcrumbs {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbDocumentWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-split-view': UmbDocumentWorkspaceSplitViewElement;
	}
}
