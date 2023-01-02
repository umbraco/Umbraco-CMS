import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-packages-created-item')
export class UmbPackagesCreatedItem extends UmbContextConsumerMixin(LitElement) {
	@property({ type: Object })
	package!: any;

	render() {
		return html`
			<uui-ref-node-package
				name=${this.package.name}
				version=${this.package.version}
				@open=${this._onClick}></uui-ref-node-package>
		`;
	}

	private _onClick() {
		window.history.pushState({}, '', `/section/packages/view/created/packageBuilder/${this.package.key}`);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-created-item': UmbPackagesCreatedItem;
	}
}
