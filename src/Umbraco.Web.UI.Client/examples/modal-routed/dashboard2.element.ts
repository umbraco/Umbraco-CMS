import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { EXAMPLE_ROUTED_MODAL } from './modal/example-modal-token';

@customElement('umb-dashboard2')
export class UmbDashboard2Element extends UmbElementMixin(LitElement) {

  constructor() {
    super();
	}

  override render() {
    return html`
      <div>
        <h2>Link to modal route</h2>
				<p>This page only shows how to link to the routed modal that is placed on a tab on the "Modal Dashboard". Clicking this link will not load the slots inside the modal, however, going to the "Modal Dashboard", clicking on tab 2 and opening the modal from there will work.</p>
				<a href="section/content/dashboard/example/tab2/modal/example-routed-modal/view/abc123/overview">Open Modal Route</a>
      </div>
    `

  }

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboard2Element

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard2': UmbDashboard2Element;
	}
}
