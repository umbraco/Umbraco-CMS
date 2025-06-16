import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-documentation-intro-dashboard-app';
@customElement(elementName)
export class UmbDocumentationIntroDashboardAppElement extends UmbLitElement {
	override render() {
		return html`
			<p>
				<umb-localize key="settingsDashboard_documentationDescription"></umb-localize>
			</p>
			<uui-button
				look="outline"
				href="https://docs.umbraco.com/umbraco-cms/umbraco-cms"
				label=${this.localize.term('settingsDashboard_getHelp')}
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

export { UmbDocumentationIntroDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentationIntroDashboardAppElement;
	}
}
