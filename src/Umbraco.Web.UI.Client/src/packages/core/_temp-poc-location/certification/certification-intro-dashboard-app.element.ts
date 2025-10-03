import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestDashboardApp,
	UmbDashboardAppElement,
	UmbDashboardAppSize,
} from '@umbraco-cms/backoffice/dashboard';

@customElement('umb-certification-intro-dashboard-app')
export class UmbCertificationIntroDashboardAppElement extends UmbLitElement implements UmbDashboardAppElement {
	@property({ type: Object })
	manifest?: ManifestDashboardApp;

	@property({ type: String })
	size?: UmbDashboardAppSize;

	override render() {
		return html`
			<umb-dashboard-app-layout headline=${this.localize.string('#settingsDashboard_trainingHeader')}>
				<p>
					<umb-localize key="settingsDashboard_trainingDescription"></umb-localize>
				</p>
				<uui-button
					look="outline"
					href="https://umbraco.com/training/"
					label=${this.localize.term('settingsDashboard_getCertified')}
					target="_blank"
					rel="noopener"></uui-button>
			</umb-dashboard-app-layout>
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
		['umb-certification-intro-dashboard-app']: UmbCertificationIntroDashboardAppElement;
	}
}
