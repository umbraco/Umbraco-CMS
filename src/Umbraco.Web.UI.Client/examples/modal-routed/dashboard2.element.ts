import { css, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-dashboard2')
export class UmbDashboard2Element extends UmbElementMixin(LitElement) {
	constructor() {
		super();
	}

	override render() {
		return html`
			<div>
				<h2>Link to modal route</h2>
				<p>
					This page only shows how to link to the routed modal that is placed on a tab on the "Modal Dashboard".
					Clicking this link will not load the slots inside the modal, however, going to the "Modal Dashboard", clicking
					on tab 2 and opening the modal from there will work.
				</p>
				<a href="section/content/dashboard/example/tab2/modal/example-routed-modal/view/abc123/">Open Modal Route</a>
			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboard2Element;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard2': UmbDashboard2Element;
	}
}
