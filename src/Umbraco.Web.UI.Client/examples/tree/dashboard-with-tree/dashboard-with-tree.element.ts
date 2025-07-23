import { EXAMPLE_TREE_ALIAS } from '../tree/constants.js';
import { html, customElement, LitElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-dashboard-with-tree')
export class ExampleDashboardWithTree extends UmbElementMixin(LitElement) {
	override render() {
		return html`<uui-box><umb-tree alias=${EXAMPLE_TREE_ALIAS}></umb-tree></uui-box>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export { ExampleDashboardWithTree as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-dashboard-with-tree': ExampleDashboardWithTree;
	}
}
