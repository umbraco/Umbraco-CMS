import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { css, html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUICardElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-template-card
 * @slot actions
 * @fires open
 * @fires selected
 */
// TODO: This should extends the UUICardElement, and the visual look of this should be like the UserCard or similarly.
// TOOD: Consider if this should be select in the 'persisted'-select style when it is selected as a default. (But its should not use the runtime-selection style)
@customElement('umb-template-card')
export class UmbTemplateCardElement extends UmbElementMixin(UUICardElement) {
	@property({ type: String })
	name = '';

	@property({ type: Boolean, reflect: true })
	default = false;

	#id = '';

	@property({ type: String })
	public override set id(newId: string) {
		this.#id = newId;
	}
	public override get id() {
		return this.#id;
	}

	// TODO: Remove in v.20
	public set value(newId: string) {
		new UmbDeprecation({
			deprecated: 'UmbTemplateCardElement.value',
			solution:
				'Use the "id" property instead. The "value" property will be removed in version 20.0.0 of the backoffice.',
			removeInVersion: '20.0.0',
		}).warn();
		this.id = newId;
	}
	public get value() {
		new UmbDeprecation({
			deprecated: 'UmbTemplateCardElement.value',
			solution:
				'Use the "id" property instead. The "value" property will be removed in version 20.0.0 of the backoffice.',
			removeInVersion: '20.0.0',
		}).warn();
		return this.#id;
	}

	#setSelection(e: KeyboardEvent) {
		e.preventDefault();
		e.stopPropagation();
		//this.selected = true;
		this.dispatchEvent(new CustomEvent('change', { bubbles: false, composed: true }));
	}
	#openTemplate(e: KeyboardEvent) {
		e.preventDefault();
		e.stopPropagation();
		this.dispatchEvent(new CustomEvent('open', { bubbles: false, composed: true }));
	}

	override render() {
		return html`<div id="card">
			${this.href ? this.#renderLink() : this.#renderButton()}
			<uui-button
				id="bottom"
				label="${this.localize.term('settings_defaulttemplate')}"
				look=${this.default ? 'default' : 'secondary'}
				?disabled="${this.default}"
				@click="${this.#setSelection}">
				${this.localize.term(this.default ? 'settings_defaulttemplate' : 'grid_setAsDefault')}
			</uui-button>
			<slot name="actions"></slot>
		</div>`;
	}

	#renderButton() {
		return html`
			<button id="open-part" aria-label="Open ${this.name}" @click="${this.#openTemplate}">
				${this.#renderContent()}
			</button>
		`;
	}

	#renderLink() {
		return html`
			<a
				id="open-part"
				aria-label="Open ${this.name}"
				tabindex=${ifDefined(!this.disabled ? 0 : undefined)}
				href=${ifDefined(!this.disabled ? this.href : undefined)}
				>${this.#renderContent()}</a
			>
		`;
	}

	#renderContent() {
		return html`
			<uui-icon class="logo" name="icon-document-html"></uui-icon>
			<div>${this.name.length ? this.name : 'Untitled template'}</div>
		`;
	}

	static override styles = [
		...UUICardElement.styles,
		css`
			:host {
				box-sizing: border-box;
				display: contents;
				position: relative;

				height: 100%;
				border: 1px solid red;
				margin: auto;
			}

			#card {
				box-sizing: border-box;
				width: 100%;
				max-width: 180px;
				position: relative;
				display: flex;
				flex-direction: column;
				align-items: stretch;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
				padding: var(--uui-size-4);
			}

			#bottom {
				margin-top: auto;
			}

			slot[name='actions'] {
				position: absolute;
				top: var(--uui-size-4);
				right: var(--uui-size-4);
				display: flex;
				justify-content: right;

				opacity: 0;
				transition: opacity 120ms;
			}

			:host(:focus) slot[name='actions'],
			:host(:focus-within) slot[name='actions'],
			:host(:hover) slot[name='actions'] {
				opacity: 1;
			}

			#open-part {
				border: none;
				outline: none;
				background: none;
				text-align: center;
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				cursor: pointer;
				flex-grow: 1;
				font-family: inherit;
				color: inherit;
				text-decoration: none;
			}

			#open-part,
			#card {
				gap: var(--uui-size-space-2);
			}

			#open-part strong {
				flex-grow: 1;
				display: flex;
				align-items: center;
			}

			:host([disabled]) #open-part {
				pointer-events: none;
			}

			#open-part:focus-visible,
			#open-part:hover {
				text-decoration: underline;
				color: var(--uui-color-interactive-emphasis);
			}

			#open-part uui-icon {
				font-size: var(--uui-size-10);
			}
		`,
	];
}

export default UmbTemplateCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-card': UmbTemplateCardElement;
	}
}
