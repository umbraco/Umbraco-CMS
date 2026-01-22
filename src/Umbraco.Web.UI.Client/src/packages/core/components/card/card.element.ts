import { property, nothing, ifDefined, html, css } from '@umbraco-cms/backoffice/external/lit';
import { defineElement, UUICardElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-card
 * @slot - slot for the default content area
 * @slot tag - slot for the tag with support for `<uui-tag>` elements
 * @slot actions - slot for the actions with support for the `<uui-action-bar>` element
 */
@defineElement('umb-card')
export class UmbCardElement extends UUICardElement {
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
        <span title="${this.name}" id="name">${this.name}</span>
        <small title="${ifDefined(this.description)}">${this.description}<slot name="description"></slot></small>
      </div></div>
    `;
	}

	static override styles = [
		...UUICardElement.styles,
		css`
			:host {
				background-color: var(--uui-color-surface-alt);
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
				display: flex;
				justify-content: center;
				min-height: 150px;
				max-height: 150px;
				width: 100%;
				margin-bottom: var(--uui-size-layout-2);
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
				position: absolute;
				z-index: 1;
				inset: 0;
				color: var(--uui-color-interactive);
				border: none;
				cursor: pointer;
				display: flex;
				flex-direction: column;
				justify-content: flex-end;
			}

			:host([disabled]) #open-part {
				pointer-events: none;
				background: var(--uui-color-disabled);
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

			:host([image]:not([image=''])) #open-part {
				transition: opacity 0.5s 0.5s;
				opacity: 0;
			}

			#content {
				position: relative;
				display: flex;
				flex-direction: column;
				width: 100%;
				font-family: inherit;
				font-size: var(--uui-type-small-size);
				box-sizing: border-box;
				text-align: left;
				word-break: break-word;
				padding-top: var(--uui-size-space-3);
			}

			#content::before {
				content: '';
				position: absolute;
				inset: 0;
				z-index: -1;
				border-top: 1px solid var(--uui-color-divider);
				border-radius: 0 0 var(--uui-border-radius) var(--uui-border-radius);
				background-color: var(--uui-color-surface);
				pointer-events: none;
				opacity: 0.96;
			}

			:host(:focus) slot[name='actions'],
			:host(:focus-within) slot[name='actions'],
			:host(:hover) slot[name='actions'] {
				opacity: 1;
			}

			:host(
					[image]:not([image='']):hover,
					[image]:not([image='']):focus,
					[image]:not([image='']):focus-within,
					[selected][image]:not([image='']),
					[error][image]:not([image=''])
				)
				#open-part {
				opacity: 1;
				transition-duration: 120ms;
				transition-delay: 0s;
			}

			:host([selectable]) #open-part {
				inset: var(--uui-size-space-3) var(--uui-size-space-4);
			}
			:host(:not([selectable])) #content {
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
			}
			:host([selectable]) #content::before {
				inset: calc(var(--uui-size-space-3) * -1) calc(var(--uui-size-space-4) * -1);
				top: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-card': UmbCardElement;
	}
}
