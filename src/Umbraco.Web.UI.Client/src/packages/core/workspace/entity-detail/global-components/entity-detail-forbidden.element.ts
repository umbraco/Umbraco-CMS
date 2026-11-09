import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-entity-detail-forbidden')
export class UmbEntityDetailForbiddenElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType = '';

	/**
	 * Replaces the default headline, together with `message`. Can contain localization keys prefixed with `#`, which
	 * receive the entity type as their argument.
	 */
	@property({ type: String })
	headline?: string;

	/**
	 * Replaces the default description when a `headline` is set. Can contain localization keys prefixed with `#`, which
	 * receive the entity type as their argument.
	 * Rendered as HTML, so it must come from a trusted source.
	 */
	@property({ type: String })
	message?: string;

	#modalContext?: UmbModalContext;

	@state()
	private _isInModal = false;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.#modalContext = context;
			this._isInModal = !!context;
		});
	}

	#close() {
		this.#modalContext?.reject();
	}

	override render() {
		return html`
			<div class="uui-text">
				${this.headline
					? html`<h4>${this.localize.string(this.headline, this.entityType)}</h4>
							${this.localize.htmlString(this.message ?? '', this.entityType)}`
					: html`<h4>${this.localize.term('entityDetail_forbiddenTitle', this.entityType)}</h4>
							${this.localize.term('entityDetail_forbiddenDescription', this.entityType)}`}
				${this._isInModal
					? html`<uui-button
							look="secondary"
							label=${this.localize.term('general_close')}
							@click=${this.#close}></uui-button>`
					: nothing}
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
				gap: var(--uui-size-space-4);
				text-align: center;
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
