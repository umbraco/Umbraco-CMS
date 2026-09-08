import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 * @element umb-sensitive-value
 * @description Renders in place of a value the current user is not permitted to see. Use it wherever a value
 * is withheld, so that the field remains visible as one that exists.
 */
@customElement('umb-sensitive-value')
export class UmbSensitiveValueElement extends UmbLitElement {
	/**
	 * Renders the message without the note on how to gain access. Use where several withheld values appear
	 * together and the note would repeat, and state that note once for the group instead.
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	short = false;

	override render() {
		return html`<em>
			<umb-localize key=${this.short ? 'content_isSensitiveValue_short' : 'content_isSensitiveValue'}></umb-localize>
		</em>`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			em {
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

export default UmbSensitiveValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-sensitive-value': UmbSensitiveValueElement;
	}
}
