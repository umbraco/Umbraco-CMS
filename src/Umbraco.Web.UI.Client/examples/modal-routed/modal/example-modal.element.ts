import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-example-modal')
export class UmbExampleModal extends UmbModalBaseElement {

	@state()
  private _routes: UmbRoute[] = [
      {
          path: `/overview`,
          component: () => import('./steps/example-modal-step1.element.js'),
          setup: (component, info) => {
          },
      },
      {
          path: `/details`,
          component: () => import('./steps/example-modal-step2.element.js'),
          setup: (component, info) => {
          },
      },
      {
        path: '',
        redirectTo: 'overview',
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
				<hr/>
				<umb-router-slot
          .routes=${this._routes}
          @init=${(event: UmbRouterSlotInitEvent) => {
            console.log('modal routes init');
          }}
          @change=${(event: UmbRouterSlotChangeEvent) => {
            console.log('modal routes change');
          }}></umb-router-slot>
			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbExampleModal;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-modal': UmbExampleModal;
	}
}
