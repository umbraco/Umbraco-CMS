import { UmbWorkspaceSplitViewContext } from './workspace-split-view.context.js';
import { css, html, customElement, property, ifDefined, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

// import local components
import './workspace-split-view-variant-selector.element.js';

/**
 *
 * Example. Document Workspace would use a Variant-component(variant component would talk directly to the workspace-context)
 * As well breadcrumbs etc.
 *
 */
@customElement('umb-workspace-split-view')
export class UmbWorkspaceSplitViewElement extends UmbLitElement {
	@property({ type: Boolean })
	displayNavigation = false;

	@property({ attribute: 'back-path' })
	public backPath?: string;

	@property({ type: Number })
	public set splitViewIndex(index: number) {
		this.splitViewContext.setSplitViewIndex(index);
	}
	public get splitViewIndex(): number {
		return this.splitViewContext.getSplitViewIndex()!;
	}

	@state()
	private _variantSelectorSlotHasContent = false;

	splitViewContext = new UmbWorkspaceSplitViewContext(this);

	#onVariantSelectorSlotChanged(e: Event) {
		this._variantSelectorSlotHasContent = (e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
	}

	override render() {
		return html`
			<umb-workspace-editor
				back-path=${ifDefined(this.backPath)}
				.hideNavigation=${!this.displayNavigation}
				.enforceNoFooter=${true}>
				<slot id="header" name="variant-selector" slot="header" @slotchange=${this.#onVariantSelectorSlotChanged}>
					${when(
						!this._variantSelectorSlotHasContent,
						() => html`<umb-workspace-split-view-variant-selector></umb-workspace-split-view-variant-selector>`,
					)}
				</slot>
				${when(
					this.displayNavigation,
					() =>
						html`<umb-workspace-entity-action-menu
							slot="action-menu"
							data-mark="workspace:action-menu"></umb-workspace-entity-action-menu>`,
				)}
				<slot name="action-menu" slot="action-menu"></slot>
			</umb-workspace-editor>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				min-width: 0;
			}

			:host(:not(:last-child)) {
				border-right: 1px solid var(--uui-color-border);
			}

			#header {
				flex: 1 1 auto;
				display: block;
			}
		`,
	];
}

export default UmbWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-split-view': UmbWorkspaceSplitViewElement;
	}
}
