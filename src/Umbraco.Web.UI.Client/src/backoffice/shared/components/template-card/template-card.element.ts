import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-template-card
 * @slot actions
 * @fires open
 * @fires selected
 *
 *
 */

@customElement('umb-template-card')
export class UmbTemplateCardElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
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
				//width: 200px;
				position: relative;
				display: flex;
				flex-direction: column;
				align-items: stretch;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
				background-color: var(--uui-color-background);
				padding: var(--uui-size-4);
			}

			:host([default]) #card {
				border: 1px solid var(--uui-color-selected);
				outline: 1px solid var(--uui-color-selected);
			}
			#card:has(uui-button:hover) {
				border: 1px solid var(--uui-color-selected);
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

	@property({ type: String })
	name = '';

	@property({ type: Boolean, reflect: true })
	default = false;

	_key = '';
	@property({ type: String })
	public set key(newKey: string) {
		this._key = newKey;
		super.value = newKey;
	}
	public get key() {
		return this._key;
	}

	protected getFormElement() {
		return undefined;
	}

	#setSelection(e: KeyboardEvent) {
		e.preventDefault();
		e.stopPropagation();
		//this.selected = true;
		this.dispatchEvent(new CustomEvent('change-default', { bubbles: true, composed: true }));
	}
	#openTemplate(e: KeyboardEvent) {
		e.preventDefault();
		e.stopPropagation();
		this.dispatchEvent(new CustomEvent('open', { bubbles: true, composed: true }));
	}

	render() {
		return html`<div id="card">
			<button id="open-part" aria-label="Open ${this.name}" @click="${this.#openTemplate}">
				<uui-icon class="logo" name="umb:layout"></uui-icon>
				<strong>${this.name.length ? this.name : 'Untitled template'}</strong>
			</button>
			<uui-button id="bottom" label="Default template" ?disabled="${this.default}" @click="${this.#setSelection}">
				${this.default ? '(Default template)' : 'Set default'}
			</uui-button>
			<slot name="actions"></slot>
		</div>`;
	}
}

export default UmbTemplateCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-card': UmbTemplateCardElement;
	}
}
