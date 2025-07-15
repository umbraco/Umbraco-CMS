import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-history-list')
export class UmbHistoryListElement extends UmbLitElement {
	override render() {
		return html`<slot></slot> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: auto 1fr auto;
				align-items: center;
				--avatar-size: calc(2em + 4px);
				gap: var(--uui-size-6);
				position: relative;
			}

			/** TODO: This doesn't work due to "display:contents" in umb-history-item, but is needed for the way I put the grid together.
			* Find a different solution so we can have the grey line that links each history item together (this is purely a visual effect, no rush)

			::slotted(*) {
				position: relative;
			}

			::slotted(*:not(:last-child)) {
				margin-bottom: calc(2 * var(--uui-size-space-3));
			}
			::slotted(*:not(:last-child))::before {
				content: '';
				border: 1px solid var(--uui-color-border);
				position: absolute;
				display: block;
				height: calc(1.5 * var(--avatar-size));
				top: var(--avatar-size);
				left: calc(-1px + var(--avatar-size) / 2);
			}
			*/
		`,
	];
}

export default UmbHistoryListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-history-list': UmbHistoryListElement;
	}
}
