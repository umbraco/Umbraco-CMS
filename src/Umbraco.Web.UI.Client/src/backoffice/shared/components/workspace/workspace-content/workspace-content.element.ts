import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import '../workspace-layout/workspace-layout.element';
import '../../variant-selector/variant-selector.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/edit/workspace-view-content-edit.element';
import './views/info/workspace-view-content-info.element';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * TODO: IMPORTANT TODO: Get rid of the content workspace. Instead we aim to get separate components that can be composed by each workspace.
 * Example. Document Workspace would use a Variant-component(variant component would talk directly to the workspace-context)
 * As well breadcrumbs etc.
 *
 */
@customElement('umb-workspace-content')
export class UmbWorkspaceContentElement extends UmbLitElement {
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

	// TODO: is this used for anything?
	@property()
	alias!: string;

	// TODO: For variants and split view, we need to be able to repeat, either this element or make this element render multiple `umb-workspace-layout`

	render() {
		return html`
			<umb-workspace-layout alias=${this.alias}>
				<div id="header" slot="header">TODO: MISSING INPUT COMPONENT</div>

				<slot name="action-menu" slot="action-menu"></slot>

				<div id="footer" slot="footer">Breadcrumbs</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbWorkspaceContentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-content': UmbWorkspaceContentElement;
	}
}
