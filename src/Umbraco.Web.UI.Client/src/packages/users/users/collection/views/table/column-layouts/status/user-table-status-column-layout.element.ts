import { html, LitElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { getLookAndColorFromUserStatus } from '../../../../../../utils.js';

@customElement('umb-user-table-status-column-layout')
export class UmbUserTableStatusColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value: any;

	render() {
		return html`${this.value.status && this.value.status !== 'enabled'
			? html`<uui-tag
					size="s"
					look="${getLookAndColorFromUserStatus(this.value.status).look}"
					color="${getLookAndColorFromUserStatus(this.value.status).color}">
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
