import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-detail-not-found')
export class UmbEntityDetailNotFoundElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	override render() {
		return html`
			<div class="uui-text">
				<h4>${this.localize.term('entityDetail_notFoundTitle', this.entityType)}</h4>
				${this.localize.term('entityDetail_notFoundDescription', this.entityType)}
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				min-width: 0;
			}

			:host > div {
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;
				height: 100%;
			}

			@keyframes fadeIn {
				100% {
					opacity: 100%;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-detail-not-found': UmbEntityDetailNotFoundElement;
	}
}
