import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestDashboardApp,
	UmbDashboardAppElement,
	UmbDashboardAppSize,
} from '@umbraco-cms/backoffice/dashboard';

const elementName = 'umb-videos-intro-dashboard-app';
@customElement(elementName)
export class UmbVideosIntroDashboardAppElement extends UmbLitElement implements UmbDashboardAppElement {
	@property({ type: Object })
	manifest?: ManifestDashboardApp;

	@property({ type: String })
	size?: UmbDashboardAppSize;

	override render() {
		return html`
			<umb-dashboard-app-layout headline=${this.localize.string('#settingsDashboard_videosHeader')}>
				<p>
					<umb-localize key="settingsDashboard_videosDescription"></umb-localize>
				</p>
				<uui-button
					look="outline"
					href="https://www.youtube.com/c/UmbracoLearningBase"
					label=${this.localize.term('settingsDashboard_watchVideos')}
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

export { UmbVideosIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbVideosIntroDashboardAppElement;
	}
}
