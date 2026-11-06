import { property, state, nothing, ifDefined, html, css } from '@umbraco-cms/backoffice/external/lit';
import { defineElement, UUICardElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-figure-card
 * @slot - slot for the default content area
 * @slot tag - slot for the tag with support for `<uui-tag>` elements
 * @slot actions - slot for the actions with support for the `<uui-action-bar>` element
 */
@defineElement('umb-figure-card')
export class UmbFigureCardElement extends UUICardElement {
	/**
	 * Name
	 * @type {string}
	 * @attr name
	 * @default ''
	 */
	@property({ type: String })
	name = '';

	/**
	 * Description
	 * @type {string}
	 * @attr description
	 * @default undefined
	 */
	@property({ type: String })
	description?: string;

	@state()
	private _hasSlottedDescription = false;

	@property({ type: String, attribute: 'background-color' })
	public get backgroundColor(): string | undefined {
		return undefined;
	}
	public set backgroundColor(value: string | undefined) {
		this.style.backgroundColor = value ?? '';
	}

	override render() {
		return html`
			${this.#renderMedia()} ${this.href ? this.#renderLink() : this.#renderButton()}
			<!-- Select border must be right after #open-part -->
			<div id="select-border"></div>
			${this.selectable ? this.renderCheckbox() : nothing}
			<slot name="tag"></slot>
			<slot name="actions"></slot>
		`;
	}

	#renderButton() {
		const tabIndex = !this.disabled ? (this.selectOnly ? -1 : 0) : undefined;
		return html`
			<button
				id="open-part"
				class="uui-text"
				tabindex=${ifDefined(tabIndex)}
				@click=${this.handleOpenClick}
				@keydown=${this.handleOpenKeydown}>
				${this.#renderContent()}
			</button>
		`;
	}

	#renderLink() {
		const tabIndex = !this.disabled ? (this.selectOnly ? -1 : 0) : undefined;
		const rel = this.target === '_blank' ? 'noopener noreferrer' : undefined;
		return html`
			<a
				id="open-part"
				class="uui-text"
				tabindex=${ifDefined(tabIndex)}
				href=${ifDefined(!this.disabled ? this.href : undefined)}
				target=${ifDefined(this.target || undefined)}
				rel=${ifDefined(this.rel || rel)}>
				${this.#renderContent()}
			</a>
		`;
	}

	#renderMedia() {
		return html`<div id="portrait">
			<slot></slot>
		</div> `;
	}

	#renderContent() {
		return html`
      <div id="content">
        <span id="name-part">
          ${this.#renderChildrenIndicator()}
          <span title="${this.name}" id="name">${this.name}</span>
        </span>
        <small title="${ifDefined(this.description)}" ?hidden=${!this.description && !this._hasSlottedDescription}>${this.description}<slot name="description" @slotchange=${this.#onDescriptionSlotChange}></slot></small>
      </div></div>
    `;
	}

	#onDescriptionSlotChange(event: Event) {
		this._hasSlottedDescription = (event.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
	}

	#renderChildrenIndicator() {
		if (!this.hasChildren) return nothing;
		return html`<uui-symbol-expand id="has-children" aria-hidden="true"></uui-symbol-expand>`;
	}

	static override styles = [
		...UUICardElement.styles,
		css`
			:host {
				height: 100%;
				flex-direction: column;
				justify-content: flex-start;
				background-color: var(--uui-color-surface-alt);
			}

			:host([disabled]) {
				background: var(--uui-color-disabled);
				color: var(--uui-color-contrast-disabled);
			}

			slot[name='tag'] {
				position: absolute;
				bottom: var(--uui-size-4);
				right: var(--uui-size-4);
				display: flex;
				justify-content: right;
				z-index: 2;
			}

			slot[name='actions'] {
				position: absolute;
				top: var(--uui-size-4);
				right: var(--uui-size-4);
				display: flex;
				justify-content: right;
				z-index: 2;
				opacity: 0;
				transition: opacity 120ms;
			}
			:host(:focus) slot[name='actions'],
			:host(:focus-within) slot[name='actions'],
			:host(:hover) slot[name='actions'] {
				opacity: 1;
			}

			#portrait {
				flex: none;
				display: flex;
				justify-content: center;
				height: var(--uui-size-40);
				width: 100%;
			}

			slot:not([name])::slotted(*) {
				align-self: center;
				border-radius: var(--uui-border-radius);
				object-fit: cover;
				max-width: 100%;
				max-height: 100%;
				font-size: var(--uui-size-8);
			}

			#open-part {
				flex: 1 0 auto;
				display: flex;
				color: var(--uui-color-interactive);
				border: none;
				cursor: pointer;
			}

			/* Stretch the click target over the whole card while the text stays in flow below the portrait. */
			#open-part::after {
				content: '';
				position: absolute;
				z-index: 1;
				inset: 0;
				border-radius: var(--uui-border-radius-2);
			}

			#open-part:focus {
				outline: none;
			}

			#open-part:focus-visible::after {
				outline: var(--uui-card-border-width) solid var(--uui-color-focus);
				outline-offset: var(--uui-card-border-width);
			}

			:host([disabled]) #open-part {
				pointer-events: none;
				color: var(--uui-color-contrast-disabled);
			}

			#open-part:hover {
				color: var(--uui-color-interactive-emphasis);
			}
			#open-part:hover #name {
				text-decoration: underline;
			}
			#open-part #name,
			#open-part small {
				display: -webkit-box;
				-webkit-line-clamp: 1;
				-webkit-box-orient: vertical;
				overflow: hidden;
				text-overflow: ellipsis;
				overflow-wrap: anywhere;
			}

			#open-part small {
				-webkit-line-clamp: 4;
				font-size: var(--uui-type-small-size);
				margin-top: var(--uui-size-space-1);
			}

			#open-part small[hidden] {
				display: none;
			}

			#name-part {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-1);
			}

			#has-children {
				flex: none;
			}

			#content {
				position: relative;
				z-index: 2;
				display: flex;
				flex-direction: column;
				width: 100%;
				font-family: inherit;
				box-sizing: border-box;
				text-align: left;
				word-break: break-word;
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				border-top: 1px solid var(--uui-color-divider);
				border-radius: 0 0 var(--uui-border-radius-2) var(--uui-border-radius-2);
				background-color: var(--uui-color-surface);
			}

			:host([disabled]) #content {
				background-color: transparent;
			}

			:host([selectable]) #open-part::after {
				inset: var(--uui-size-space-3) var(--uui-size-space-4);
			}

			/* Let clicks on the card's outer edge reach the host, so they select instead of open. */
			:host([selectable]) #open-part,
			:host([selectable]) #content {
				pointer-events: none;
			}
			:host([selectable]:not([select-only])) #open-part::after,
			:host([selectable]:not([select-only])) #content > * {
				pointer-events: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-figure-card': UmbFigureCardElement;
	}
}
