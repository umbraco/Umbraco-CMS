import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-scrollable-container')
export class UmbScrollableContainerElement extends UmbLitElement {
	@state()
	private _showScrollLeft = false;

	@state()
	private _showScrollRight = false;

	#resizeObserver = new ResizeObserver(() => this.#updateScrollButtons());
	#resizeObserverConnected = false;

	#updateScrollButtons() {
		this._showScrollLeft = this.scrollLeft > 0;
		// -1 tolerance accounts for subpixel rounding differences across browsers.
		this._showScrollRight = this.scrollLeft + this.clientWidth < this.scrollWidth - 1;
	}

	#scrollLeft() {
		this.scrollBy({ left: -200, behavior: 'smooth' });
	}

	#scrollRight() {
		this.scrollBy({ left: 200, behavior: 'smooth' });
	}

	override updated() {
		if (!this.#resizeObserverConnected) {
			this.#resizeObserver.observe(this);
			this.#resizeObserverConnected = true;
		}
	}

	override connectedCallback() {
		super.connectedCallback();
		this.addEventListener('scroll', this.#updateScrollButtons);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.removeEventListener('scroll', this.#updateScrollButtons);
		this.#resizeObserver.disconnect();
		this.#resizeObserverConnected = false;
	}

	override destroy() {
		this.#resizeObserver.disconnect();
		super.destroy();
	}

	override render() {
		return html`
			${this._showScrollLeft
				? html`<uui-button
						id="scroll-left"
						compact
						@click=${this.#scrollLeft}
						label=${this.localize.term('general_scrollLeft')}>
						<uui-icon name="icon-arrow-left"></uui-icon>
					</uui-button>`
				: nothing}
			<slot></slot>
			${this._showScrollRight
				? html`<uui-button
						id="scroll-right"
						compact
						@click=${this.#scrollRight}
						label=${this.localize.term('general_scrollRight')}>
						<uui-icon name="icon-arrow-right"></uui-icon>
					</uui-button>`
				: nothing}
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				position: relative;
				overflow-x: auto;
				min-width: 0;
				align-items: center;
				scrollbar-width: none;
			}

			:host::-webkit-scrollbar {
				display: none;
			}

			#scroll-left,
			#scroll-right {
				flex-shrink: 0;
				position: sticky;
				z-index: 1;
				background-color: var(--uui-color-surface);
				height: 100%;
			}

			#scroll-left {
				left: 0;
			}

			#scroll-right {
				right: 0;
			}

			::slotted(*) {
				flex-shrink: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-scrollable-container': UmbScrollableContainerElement;
	}
}
