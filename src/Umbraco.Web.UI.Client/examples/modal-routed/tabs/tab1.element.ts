import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { EXAMPLE_ROUTED_MODAL } from './modal/example-modal-token';

@customElement('umb-dashboard-tab1')
export class UmbDashboardTab1Element extends UmbElementMixin(LitElement) {

	#workspaceModal?: UmbModalRouteRegistrationController;

  @state()
	_editLinkPath?: string;

  constructor() {
    super();

    }

  override render() {
    return html`
      <div>
        <h2>tab 1</h2>

      </div>
    `

  }

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardTab1Element

declare global {
	interface UmbDashboardTab1Element {
		'umb-dashboard-tab1': UmbDashboardTab1Element;
	}
}
