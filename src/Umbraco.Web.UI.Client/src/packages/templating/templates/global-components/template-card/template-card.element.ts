import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-template-card
 * @slot actions
 * @fires open
 * @fires selected
 */
// TODO: This should extends the UUICardElement, and the visual look of this should be like the UserCard or similarly.
// TOOD: Consider if this should be select in the 'persisted'-select style when it is selected as a default. (But its should not use the runtime-selection style)
@customElement('umb-template-card')
export class UmbTemplateCardElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ type: String })
	override name = '';

	@property({ type: Boolean, reflect: true })
	default = false;

	_id = '';
	@property({ type: String })
	public override set id(newId: string) {
		this._id = newId;
		super.value = newId;
	}
	public override get id() {
		return this._id;
	}

	protected override getFormElement() {
		return undefined;
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
			<button id="open-part" aria-label="Open ${this.name}" @click="${this.#openTemplate}">
				<uui-icon class="logo" name="icon-document-html"></uui-icon>
				<strong>${this.name.length ? this.name : 'Untitled template'}</strong>
			</button>
			<uui-button
				id="bottom"
				label="${this.localize.term('settings_defaulttemplate')}"
				?disabled="${this.default}"
				@click="${this.#setSelection}">
				(${this.localize.term(this.default ? 'settings_defaulttemplate' : 'grid_setAsDefault')})
			</uui-button>
			<slot name="actions"></slot>
		</div>`;
	}

	static override styles = [
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
				border: 1px solid var(--uui-color-divider-emphasis);
				background-color: var(--uui-color-background);
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
				font-weight: 700;
				align-items: center;
				cursor: pointer;
				flex-grow: 1;
				font-family: inherit;
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
			#open-part:focus-visible uui-icon,
			#open-part:hover,
			#open-part:hover uui-icon {
				text-decoration: underline;
				color: var(--uui-color-interactive-emphasis);
			}

			#open-part uui-icon {
				font-size: var(--uui-size-20);
				color: var(--uui-color-divider-emphasis);
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
