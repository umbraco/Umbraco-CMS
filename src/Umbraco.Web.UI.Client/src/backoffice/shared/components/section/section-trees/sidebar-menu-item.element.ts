import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestTree } from '@umbraco-cms/models';

@customElement('umb-sidebar-menu-item')
export class UmbSidebarMenuItem extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	public manifest?: ManifestTree;

	render() {
		return html`<uui-menu-item label="${ifDefined(this.manifest?.meta.label)}">
			<uui-icon slot="icon" name="${ifDefined(this.manifest?.meta.icon)}"></uui-icon>
		</uui-menu-item> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-sidebar-menu-item': UmbSidebarMenuItem;
	}
}
