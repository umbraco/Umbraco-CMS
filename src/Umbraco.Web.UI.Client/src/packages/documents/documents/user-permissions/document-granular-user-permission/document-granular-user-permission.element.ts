import type { UmbDocumentUserPermissionModel } from '../types.js';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-granular-user-permission')
export class UmbDocumentGranularUserPermissionElement extends UmbLitElement {
	@property({ type: Array, attribute: false })
	value: Array<UmbDocumentUserPermissionModel> = [];

	render() {
		return html`${this.value.map(
			(permission) => html`
				<div>${permission.$type}</div>
				<div>${permission.document.id}</div>
			`,
		)}`;
	}

	static styles = [css``];
}

export default UmbDocumentGranularUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-granular-user-permission': UmbDocumentGranularUserPermissionElement;
	}
}
