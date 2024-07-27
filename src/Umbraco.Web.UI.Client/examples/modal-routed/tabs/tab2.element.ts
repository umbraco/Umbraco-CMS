import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { EXAMPLE_ROUTED_MODAL } from './../modal/example-modal-token';

@customElement('umb-dashboard-tab2')
export class UmbDashboardTab2Element extends UmbElementMixin(LitElement) {

	#workspaceModal?: UmbModalRouteRegistrationController;

  @state()
	_editLinkPath?: string;

  constructor() {
    super();

    // Using workspace modal context
    this.#workspaceModal?.destroy();
      this.#workspaceModal = new UmbModalRouteRegistrationController(this, EXAMPLE_ROUTED_MODAL)
        .addAdditionalPath('view/:entityKey')
        .onSetup(() => {
          return {
						data: {},
						value: {}
					};
        })
        .observeRouteBuilder((routeBuilder) => {
          this._editLinkPath = routeBuilder({entityKey : 'abc123'});
        });
    }

  override render() {
    return html`
      <div>
        <h2>tab 2</h2>
				<p>This element hosts the UmbModalRouteRegistrationController</p>

        <a href=${this._editLinkPath ?? ""}>Open modal</a>
        <hr/>

        Path: ${this._editLinkPath}
      </div>
    `

  }

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardTab2Element

declare global {
	interface UmbDashboardTab2Element {
		'umb-dashboard-tab2': UmbDashboardTab2Element;
	}
}
