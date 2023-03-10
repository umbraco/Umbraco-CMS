import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { ActiveVariant } from '../../../shared/components/workspace/workspace-context/workspace-split-view-manager.class';
import { UmbDocumentWorkspaceContext } from './document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import '../../../shared/components/workspace/workspace-variant/workspace-variant.element';

@customElement('umb-document-workspace-split-view')
export class UmbDocumentWorkspaceSplitViewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
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
				height: 100%;
			}

			#breadcrumbs {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	@state()
	_unique?: string;

	@state()
	_variants?: Array<ActiveVariant>;

	constructor() {
		super();

		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (context) => {
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
							(view) => view.index,
							(view) => html`
								<umb-workspace-variant
									alias="Umb.Workspace.Document"
									.splitViewIndex=${view.index}
									.displayNavigation=${view.index === this._variants!.length - 1}></umb-workspace-variant>
							`
						)}
					</div>

					<umb-workspace-footer-layout alias="Umb.Workspace.Document">
						<div id="breadcrumbs">Breadcrumbs</div>
					</umb-workspace-footer-layout>`
			: nothing;
	}
}

export default UmbDocumentWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-split-view': UmbDocumentWorkspaceSplitViewElement;
	}
}
