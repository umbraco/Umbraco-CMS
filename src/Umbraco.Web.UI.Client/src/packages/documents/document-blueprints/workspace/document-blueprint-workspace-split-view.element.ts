import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS, UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT } from './constants.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-workspace-split-view')
export class UmbDocumentBlueprintWorkspaceSplitViewElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_variants?: Array<ActiveVariant>;

	@state()
	_isNew?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.#observeActiveVariantInfo();
			this.#observeIsNew();
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
		return view.index === this._variants!.length - 1 && this._isNew === false;
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
									.displayNavigation=${this.#getDisplayNavigation(view)}></umb-workspace-split-view>
							`,
						)}
					</div>

					<umb-workspace-footer alias=${UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS}></umb-workspace-footer>`
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

export default UmbDocumentBlueprintWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-workspace-split-view': UmbDocumentBlueprintWorkspaceSplitViewElement;
	}
}
