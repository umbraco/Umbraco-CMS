import { UmbWorkspaceSplitViewContext } from './workspace-split-view.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 *
 * Example. Document Workspace would use a Variant-component(variant component would talk directly to the workspace-context)
 * As well breadcrumbs etc.
 *
 */
@customElement('umb-workspace-split-view')
export class UmbWorkspaceSplitViewElement extends UmbLitElement {
	// TODO: stop prop drilling this alias. Instead use the workspace context.
	@property()
	alias!: string;

	@property({ type: Boolean })
	displayNavigation = false;

	@property({ type: Number })
	public set splitViewIndex(index: number) {
		this.splitViewContext.setSplitViewIndex(index);
	}

	splitViewContext = new UmbWorkspaceSplitViewContext(this);

	render() {
		return html`
			<umb-workspace-editor
				alias=${this.alias}
				.hideNavigation=${!this.displayNavigation}
				.enforceNoFooter=${true}>
				<div id="header" slot="header">
					<umb-variant-selector></umb-variant-selector>
				</div>
				${this.displayNavigation
					? html`<umb-workspace-action-menu slot="action-menu"></umb-workspace-action-menu>`
					: ''}
				<slot name="action-menu" slot="action-menu"></slot>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			:host(:not(:last-child)) {
				border-right: 1px solid var(--uui-color-border);
			}

			#header {
				flex: 1 1 auto;
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
