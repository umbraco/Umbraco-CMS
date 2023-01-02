import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-user-dashboard-test')
export class UmbUserDashboardTestElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-5);
				border: 1px solid var(--uui-color-border);
				background: var(--uui-color-positive);
				color: var(--uui-color-positive-contrast);
				border-radius: var(--uui-border-radius);
			}
			p {
				margin: 0;
			}
		`,
	];

	render() {
		return html`
			<b>Custom User Dashboard</b>
			<p>This is an example of a custom user dashboard using the user dashboard extension point</p>
		`;
	}
}

export default UmbUserDashboardTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-dashboard-test': UmbUserDashboardTestElement;
	}
}
