import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import { html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-user-table-status-column-layout')
export class UmbUserTableStatusColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value: any;

	public _getTagLookAndColor(status?: string): { color: InterfaceColor; look: InterfaceLook } {
		switch ((status || '').toLowerCase()) {
			case 'invited':
			case 'inactive':
				return { look: 'primary', color: 'warning' };
			case 'active':
				return { look: 'primary', color: 'positive' };
			case 'disabled':
				return { look: 'primary', color: 'danger' };
			default:
				return { look: 'secondary', color: 'default' };
		}
	}

	render() {
		return html`${this.value.status && this.value.status !== 'Enabled'
			? html`<uui-tag
					size="s"
					look="${this._getTagLookAndColor(this.value.status).look}"
					color="${this._getTagLookAndColor(this.value.status).color}">
					${this.value.status}
			  </uui-tag>`
			: nothing}`;
	}
}

export default UmbUserTableStatusColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-table-status-column-layout': UmbUserTableStatusColumnLayoutElement;
	}
}
