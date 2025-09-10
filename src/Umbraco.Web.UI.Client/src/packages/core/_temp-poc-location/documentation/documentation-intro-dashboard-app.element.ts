import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestDashboardApp,
	UmbDashboardAppElement,
	UmbDashboardAppSize,
} from '@umbraco-cms/backoffice/dashboard';

const elementName = 'umb-documentation-intro-dashboard-app';
@customElement(elementName)
export class UmbDocumentationIntroDashboardAppElement extends UmbLitElement implements UmbDashboardAppElement {
	@property({ type: Object })
	manifest?: ManifestDashboardApp;

	@property({ type: String })
	size?: UmbDashboardAppSize;

	override render() {
		return html`
			<umb-dashboard-app-layout headline=${this.localize.string('#settingsDashboard_documentationHeader')}>
				<p>
					<umb-localize key="settingsDashboard_documentationDescription"></umb-localize>
				</p>
				<uui-button
					look="outline"
					href="https://docs.umbraco.com/umbraco-cms/umbraco-cms"
					label=${this.localize.term('settingsDashboard_getHelp')}
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

export { UmbDocumentationIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentationIntroDashboardAppElement;
	}
}
