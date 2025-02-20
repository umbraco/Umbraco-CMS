import { EXAMPLE_ROUTED_MODAL } from '../modal/example-modal-token.js';
import { css, html, LitElement, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';

@customElement('umb-dashboard-tab2')
export class UmbDashboardTab2Element extends UmbElementMixin(LitElement) {
	#workspaceModal?: UmbModalRouteRegistrationController<
		typeof EXAMPLE_ROUTED_MODAL.DATA,
		typeof EXAMPLE_ROUTED_MODAL.VALUE
	>;

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
					value: {},
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editLinkPath = routeBuilder({ entityKey: 'abc123' });
			});
	}

	override render() {
		return html`
			<div>
				<h2>tab 2</h2>
				<p>This element hosts the UmbModalRouteRegistrationController</p>

				<a href=${this._editLinkPath ?? ''}>Open modal</a>
			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardTab2Element;

declare global {
	interface UmbDashboardTab2Element {
		'umb-dashboard-tab2': UmbDashboardTab2Element;
	}
}
