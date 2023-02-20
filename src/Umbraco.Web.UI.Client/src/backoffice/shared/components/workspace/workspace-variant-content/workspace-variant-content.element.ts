import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import '../workspace-layout/workspace-layout.element';
import '../../variant-selector/variant-selector.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/edit/workspace-view-content-edit.element';
import './views/info/workspace-view-content-info.element';
import { UmbVariantContentContext } from './variant-content.context';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 *
 * Example. Document Workspace would use a Variant-component(variant component would talk directly to the workspace-context)
 * As well breadcrumbs etc.
 *
 */
@customElement('umb-workspace-variant-content')
export class UmbWorkspaceVariantContentElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}

			#footer {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];

	// TODO: stop prop drilling this alias. Instead use the workspace context.
	@property()
	alias!: string;

	// Use this for any sub url routing, or maybe we should use the culture + segment for this.
	@property({ type: Number })
	public set splitViewIndex(index: number) {
		this.variantContext.setSplitViewIndex(index);
	}

	variantContext = new UmbVariantContentContext(this);

	render() {
		return html`
			<umb-workspace-layout alias=${this.alias}>
				<div id="header" slot="header">
					<umb-variant-selector></umb-variant-selector>
				</div>

				<slot name="action-menu" slot="action-menu"></slot>

				<div id="footer" slot="footer">Breadcrumbs</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbWorkspaceVariantContentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-variant-content': UmbWorkspaceVariantContentElement;
	}
}
