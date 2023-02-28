import { v4 as uuidv4 } from 'uuid';
import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestMenuItem } from '@umbraco-cms/models';

@customElement('umb-sidebar-menu-item')
export class UmbSidebarMenuItem extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _key = uuidv4();

	@property({ type: Object, attribute: false })
	manifest!: ManifestMenuItem;

	render() {
		return html`<umb-tree-item
			.key="${this._key}"
			.label=${this.manifest.meta.label || this.manifest.name}
			.icon=${this.manifest.meta.icon}
			.entityType=${this.manifest.meta.entityType}></umb-tree-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-sidebar-menu-item': UmbSidebarMenuItem;
	}
}
