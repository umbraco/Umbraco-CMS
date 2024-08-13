import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-icons-dashboard')
export class ExampleIconsDashboard extends UmbElementMixin(LitElement) {
	override render() {
		return html`
			<uui-box class="uui-text">
				<h1 class="uui-h2" style="margin-top: var(--uui-size-layout-1);">Custom icons:</h1>
				<uui-icon name="my-icon-bomb"></uui-icon>
				<uui-icon name="my-icon-bones"></uui-icon>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export default ExampleIconsDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-icons-dashboard': ExampleIconsDashboard;
	}
}
