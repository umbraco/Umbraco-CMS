import { css, html, LitElement, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-dashboard-tab1')
export class UmbDashboardTab1Element extends UmbElementMixin(LitElement) {
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
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardTab1Element;

declare global {
	interface UmbDashboardTab1Element {
		'umb-dashboard-tab1': UmbDashboardTab1Element;
	}
}
