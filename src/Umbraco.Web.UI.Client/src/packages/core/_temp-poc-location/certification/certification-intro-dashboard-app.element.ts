import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-training-intro-dashboard-app';
@customElement(elementName)
export class UmbCertificationIntroDashboardAppElement extends UmbLitElement {
	override render() {
		return html`
			<p>
				<umb-localize key="settingsDashboard_trainingDescription"></umb-localize>
			</p>
			<uui-button
				look="outline"
				href="https://umbraco.com/training/"
				label=${this.localize.term('settingsDashboard_getCertified')}
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

export { UmbCertificationIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCertificationIntroDashboardAppElement;
	}
}
