import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dashboard-app-layout')
export class UmbDashboardAppLayoutElement extends UmbLitElement {
	@property({ type: String })
	headline: string | null = null;

	override render() {
		return html`<uui-box .headline=${this.headline}><slot></slot></uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box {
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		['umb-dashboard-app-layout']: UmbDashboardAppLayoutElement;
	}
}
