import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-workspace-split-view')
export class UmbDocumentWorkspaceSplitViewElement extends UmbLitElement {
	// TODO: Refactor: use the split view context token:
	private _workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_variants?: Array<ActiveVariant>;

	constructor() {
		super();

		// TODO: Refactor: use a split view workspace context token:
		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this._observeActiveVariantInfo();
		});
	}

	private _observeActiveVariantInfo() {
		if (!this._workspaceContext) return;
		this.observe(
			this._workspaceContext.splitView.activeVariantsInfo,
			(variants) => {
				this._variants = variants;
			},
			'_observeActiveVariantsInfo',
		);
	}

	render() {
		if (!this._variants) return nothing;

		//TODO: This can probably be cleaned up.
		if (this._variants.length === 1)
			return html`
				<div id="splitViews">
					${repeat(
						this._variants,
						(view) =>
							view.index + '_' + (view.culture ?? '') + '_' + (view.segment ?? '') + '_' + this._variants!.length,
						(view) => html`
							<umb-workspace-split-view
								alias="Umb.Workspace.Document"
								.splitViewIndex=${view.index}
								.displayNavigation=${view.index === this._variants!.length - 1}></umb-workspace-split-view>
						`,
					)}
				</div>

				<umb-workspace-footer alias="Umb.Workspace.Document">
					<div id="breadcrumbs">Breadcrumbs</div>
				</umb-workspace-footer>
			`;

		return html`<div id="splitViews">
				<umb-split-panel snap="50%">
					${repeat(
						this._variants,
						(view) =>
							view.index + '_' + (view.culture ?? '') + '_' + (view.segment ?? '') + '_' + this._variants!.length,
						(view, index) => html`
							<umb-workspace-split-view
								slot=${['start', 'end'][index] || ''}
								alias="Umb.Workspace.Document"
								.splitViewIndex=${view.index}
								.displayNavigation=${view.index === this._variants!.length - 1}></umb-workspace-split-view>
						`,
					)}
				</umb-split-panel>
			</div>

			<umb-workspace-footer alias="Umb.Workspace.Document">
				<div id="breadcrumbs">Breadcrumbs</div>
			</umb-workspace-footer>`;
	}

	static styles = [
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
				width: 100%;
				height: calc(100% - var(--umb-footer-layout-height));
			}

			umb-split-panel {
				--umb-split-panel-start-min-width: 25%;
				--umb-split-panel-end-min-width: 25%;
				--umb-split-panel-divider-color: var(--uui-color-border);
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
