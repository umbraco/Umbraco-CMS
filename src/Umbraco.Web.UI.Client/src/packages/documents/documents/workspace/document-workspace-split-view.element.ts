import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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
				console.log("______variants: ", variants[0])
			},
			'_observeActiveVariantsInfo',
		);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		console.log("connected callback------")
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		console.log("DISconnected callback------")
	}

	override destroy(): void {
		super.destroy();
		console.log("split view ot destroyed")
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
									alias="Umb.Workspace.Document"
									.splitViewIndex=${view.index}
									.displayNavigation=${view.index === this._variants!.length - 1}></umb-workspace-split-view>
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
