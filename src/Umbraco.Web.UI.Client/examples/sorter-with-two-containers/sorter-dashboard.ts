import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import './sorter-group.js';
@customElement('example-sorter-dashboard')
export class ExampleSorterDashboard extends UmbElementMixin(LitElement) {
	render() {
		return html`
			<uui-box class="uui-text outer-wrapper">
				<example-sorter-group></example-sorter-group>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.outer-wrapper {
				display: flex;
				flex-direction: row;
			}
		`,
	];
}

export default ExampleSorterDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-dashboard': ExampleSorterDashboard;
	}
}
