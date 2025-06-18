import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-support-intro-dashboard-app';
@customElement(elementName)
export class UmbSupportIntroDashboardAppElement extends UmbLitElement {
	override render() {
		return html`
			<p>
				<umb-localize key="settingsDashboard_supportDescription"></umb-localize>
			</p>
			<uui-button
				look="outline"
				href="https://umbraco.com/support/"
				label=${this.localize.term('settingsDashboard_getSupport')}
				target="_blank"
				rel="noopener"></uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			p {
				margin-top: 0;
			}
		`,
	];
}

export { UmbSupportIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSupportIntroDashboardAppElement;
	}
}
