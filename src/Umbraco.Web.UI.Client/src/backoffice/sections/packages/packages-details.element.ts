import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../../core/context';

@customElement('umb-packages-details')
export class UmbPackagesDetails extends UmbContextConsumerMixin(LitElement) {
	@property()
	id!: string;

	connectedCallback() {
		super.connectedCallback();
		console.log(this.id);
	}

	render() {
		return html` <h1>Showing details of ${this.id}</h1> `;
	}
}

export default UmbPackagesDetails;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-details': UmbPackagesDetails;
	}
}
