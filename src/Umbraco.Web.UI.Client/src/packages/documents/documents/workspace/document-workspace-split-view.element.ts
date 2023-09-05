import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-document-workspace-split-view')
export class UmbDocumentWorkspaceSplitViewElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_unique?: string;

	@state()
	_variants?: Array<ActiveVariant>;

	constructor() {
		super();

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
			'_observeActiveVariantsInfo'
		);
	}

	render() {
		return this._variants
			? html`<div id="splitViews">
						${repeat(
							this._variants,
							(view) =>
								view.index + '_' + (view.culture ?? '') + '_' + (view.segment ?? '') + '_' + this._variants!.length,
							(view) => html`
								<umb-workspace-variant
									alias="Umb.Workspace.Document"
									.splitViewIndex=${view.index}
									.displayNavigation=${view.index === this._variants!.length - 1}></umb-workspace-variant>
							`
						)}
					</div>

					<umb-workspace-footer alias="Umb.Workspace.Document">
						<div id="breadcrumbs">Breadcrumbs</div>
					</umb-workspace-footer>`
			: nothing;
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
