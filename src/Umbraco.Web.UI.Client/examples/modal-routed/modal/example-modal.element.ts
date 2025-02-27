import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-example-modal')
export class UmbExampleModal extends UmbModalBaseElement {
	@state()
	private _routes: UmbRoute[] = [
		{
			path: `modalOverview`,
			component: () => import('./steps/example-modal-step1.element.js'),
		},
		{
			path: `details`,
			component: () => import('./steps/example-modal-step2.element.js'),
		},
		{
			path: '',
			redirectTo: 'modalOverview',
		},
	];

	override render() {
		return html`
			<div>
				umb-example modal element
				<hr />
				<umb-router-slot .routes=${this._routes}></umb-router-slot>
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
