import { css, customElement, html, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUICardElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-template-card
 * @slot actions
 * @fires open — when no href is set
 * @fires selected
 */
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

	/** @deprecated Use `id` instead. This property will be removed in Umbraco 20. */
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
		return html`
			<div id="card">
				${this.href ? this.#renderLink() : this.#renderButton()}
				<uui-button
					id="bottom"
					label=${this.localize.term('settings_defaulttemplate')}
					look=${this.default ? 'default' : 'secondary'}
					?disabled=${this.default}
					@click=${this.#setSelection}>
					${this.localize.term(this.default ? 'settings_defaulttemplate' : 'grid_setAsDefault')}
				</uui-button>
				<slot name="actions"></slot>
			</div>
		`;
	}

	#renderButton() {
		return html`
			<button id="open-part" aria-label="Open ${this.name}" @click=${this.#openTemplate}>
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
				href=${this.href!}>
				${this.#renderContent()}
			</a>
		`;
	}

	#renderContent() {
		return html`
			<uui-icon class="logo" name="icon-document-html"></uui-icon>
			<div>${this.name.length ? this.name : 'Untitled template'}</div>
		`;
	}

	static override readonly styles = [
		...UUICardElement.styles,
		css`
			:host {
				display: contents;
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
