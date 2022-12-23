import { html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { getTagLookAndColor } from '../../../../../../../utils';

@customElement('umb-user-table-status-column-layout')
export class UmbUserTableStatusColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value: any;

	render() {
		return html`${this.value.status && this.value.status !== 'enabled'
			? html`<uui-tag
					size="s"
					look="${getTagLookAndColor(this.value.status).look}"
					color="${getTagLookAndColor(this.value.status).color}">
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
