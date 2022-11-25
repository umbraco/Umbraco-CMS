import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-external-login-provider-test')
export class UmbExternalLoginProviderTestElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [UUITextStyles, css``];

	render() {
		return html` <div>MY CUSTOM EXTENSION LETS GOO</div> `;
	}
}

export default UmbExternalLoginProviderTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-provider-test': UmbExternalLoginProviderTestElement;
	}
}
