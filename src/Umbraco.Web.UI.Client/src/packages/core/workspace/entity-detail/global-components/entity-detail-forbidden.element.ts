import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbReason } from '@umbraco-cms/backoffice/utils';
import { css, html, customElement, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-detail-forbidden')
export class UmbEntityDetailForbiddenElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	/**
	 * Replaces the default text. Localization keys prefixed with `#` receive the entity type as their argument.
	 */
	@property({ type: Object, attribute: false })
	reason?: UmbReason;

	override render() {
		if (this.reason) {
			return html`
				<div class="uui-text">
					<h4>${this.localize.string(this.reason.title, this.entityType)}</h4>
					${this.reason.detail ? this.localize.string(this.reason.detail, this.entityType) : nothing}
				</div>
			`;
		}
		return html`
			<div class="uui-text">
				<h4>${this.localize.term('entityDetail_forbiddenTitle', this.entityType)}</h4>
				${this.localize.term('entityDetail_forbiddenDescription', this.entityType)}
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
		'umb-entity-detail-forbidden': UmbEntityDetailForbiddenElement;
	}
}
