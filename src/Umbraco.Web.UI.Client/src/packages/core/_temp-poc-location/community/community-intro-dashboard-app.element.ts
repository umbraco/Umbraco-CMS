import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-community-intro-dashboard-app';
@customElement(elementName)
export class UmbCommunityIntroDashboardAppElement extends UmbLitElement {
	override render() {
		return html`
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
