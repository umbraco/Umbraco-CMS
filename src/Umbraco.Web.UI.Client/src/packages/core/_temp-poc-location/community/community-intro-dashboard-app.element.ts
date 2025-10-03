import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestDashboardApp,
	UmbDashboardAppElement,
	UmbDashboardAppSize,
} from '@umbraco-cms/backoffice/dashboard';

const elementName = 'umb-community-intro-dashboard-app';
@customElement(elementName)
export class UmbCommunityIntroDashboardAppElement extends UmbLitElement implements UmbDashboardAppElement {
	@property({ type: Object })
	manifest?: ManifestDashboardApp;

	@property({ type: String })
	size?: UmbDashboardAppSize;

	override render() {
		return html`
			<umb-dashboard-app-layout headline=${this.localize.string('#settingsDashboard_communityHeader')}>
				<p>
					<umb-localize key="settingsDashboard_communityDescription"></umb-localize>
				</p>
				<uui-button
					look="outline"
					href="https://our.umbraco.com/forum"
					label=${this.localize.term('settingsDashboard_goForum')}
					target="_blank"
					rel="noopener"></uui-button>
				<uui-button
					look="outline"
					href="https://discord.umbraco.com"
					label=${this.localize.term('settingsDashboard_chatWithCommunity')}
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

export { UmbCommunityIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCommunityIntroDashboardAppElement;
	}
}
