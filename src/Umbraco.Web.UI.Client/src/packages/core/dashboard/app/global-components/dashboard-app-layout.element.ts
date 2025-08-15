import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dashboard-app-layout')
export class UmbDashboardAppLayoutElement extends UmbLitElement {
	@property({ type: String })
	headline: string | null = null;

	override render() {
		return html`<uui-box .headline=${this.headline}>${this.#renderActions()}<slot></slot></uui-box>`;
	}

	#renderActions() {
		return html`<div slot="header-actions">
			<umb-entity-actions-dropdown compact>
				<uui-symbol-more slot="label" label=${this.localize.term('general_actions')}></uui-symbol-more>
			</umb-entity-actions-dropdown>
		</div>`;
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
