import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-example-modal')
export class UmbExampleModal extends UmbModalBaseElement {
	@state()
	private _routes: UmbRoute[] = [];

	/**
	 *
	 */
	constructor() {
		super();
		console.log('modal element loaded');
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this._routes = [
			{
				path: `overview`,
				component: () => import('./steps/example-modal-step1.element.js'),
			},
			{
				path: `details`,
				component: () => import('./steps/example-modal-step2.element.js'),
			},
			// NL: There is a problem with this one, but there is more problems as the modal does not close when navigating the browser history.
			/*{
				path: '',
				redirectTo: 'overview',
			},*/
		];
	}

	override render() {
		return html`
			<div>
				umb-example modal
				<hr />
				<umb-router-slot
					.routes=${this._routes}
					@init=${(event: UmbRouterSlotInitEvent) => {
						console.log('modal route init fired', event);
					}}
					@change=${(event: UmbRouterSlotChangeEvent) => {
						console.log('modal routes change', event);
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
