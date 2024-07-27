import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalRouteRegistrationController, UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { EXAMPLE_ROUTED_MODAL } from './modal/example-modal-token';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbElementMixin(LitElement) {

	#workspaceModal?: UmbModalRouteRegistrationController;

  @state()
  private _routes: UmbRoute[] = [
      {
          path: `/tab1`,
          component: () => import('./tabs/tab1.element.js'),
          setup: (component, info) => {
          },
      },
      {
          path: `/tab2`,
          component: () => import('./tabs/tab2.element.js'),
          setup: (component, info) => {
          },
      },
      {
        path: '',
        redirectTo: 'tab1',
      },
  ];

	/**
	 *
	 */
	constructor() {
		super();
		console.log('modal element loaded');
	}

	override render() {
		return html`
			<div>
				umb-example modal
				<ul>
					<li><a href="section/content/dashboard/example/tab1">Tab 1</a></li>
					<li><a href="section/content/dashboard/example/tab2">Tab 2 (with modal)</a></li>
				</ul>
				<hr/>
				<umb-router-slot
          .routes=${this._routes}
          @init=${(event: UmbRouterSlotInitEvent) => {
            console.log('tab routes init');
          }}
          @change=${(event: UmbRouterSlotChangeEvent) => {
            console.log('modal routes change');
          }}></umb-router-slot>
			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardElement

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard': UmbDashboardElement;
	}
}
