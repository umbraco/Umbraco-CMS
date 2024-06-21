import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-section-main')
export class UmbSectionMainElement extends LitElement {
	override render() {
		return html`
			<main>
				<slot></slot>
			</main>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				min-width: 0;
			}

			main {
				position: relative;
				display: flex;
				flex-direction: column;
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-main': UmbSectionMainElement;
	}
}
