import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIInputElement, UUITagElement } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-input-tags')
export class UmbInputTagsElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				box-sizing: border-box;
			}

			#wrapper {
				display: flex;
				gap: var(--uui-size-space-2);
				flex-wrap: wrap;
				align-items: center;
				padding: var(--uui-size-space-2);
				border: 1px solid var(--uui-color-border);
				background-color: var(--uui-input-background-color, var(--uui-color-surface));
				flex: 1;
			}

			uui-tag uui-icon {
				cursor: pointer;
			}

			uui-tag {
				max-width: 150px;
			}

			uui-tag uui-icon {
				min-width: 12.8px !important;
			}

			uui-tag span {
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}

			#tag-add-wrapper {
				padding: 3px 4px;
				background-color: var(--uui-color-selected-contrast);
				//transition: width 500ms ease-in;
				width: 20px;
				position: relative;
			}

			#tag-add-wrapper:has(#tag-input:not(:focus)):hover {
				cursor: pointer;
				border: 1px solid var(--uui-color-selected-emphasis);
			}

			#tag-add-wrapper:has(*:hover),
			#tag-add-wrapper:has(*:active),
			#tag-add-wrapper:has(*:focus) {
				border: 1px solid var(--uui-color-selected-emphasis);
			}

			#tag-add-wrapper #tag-input:not(:focus) {
				opacity: 0;
			}

			#tag-add-wrapper:has(#tag-input:focus),
			#tag-add-wrapper:has(#tag-input:not(:placeholder-shown)) {
				width: 150px;
			}

			#tag-add-wrapper uui-icon {
				position: absolute;
				top: 50%;
				left: 50%;
				transform: translate(-50%, -50%);
			}

			#tag-add-wrapper:hover uui-icon,
			#tag-add-wrapper:active uui-icon {
				color: var(--uui-color-selected);
			}

			#tag-add-wrapper #tag-input {
				box-sizing: border-box;
				max-height: 25.8px;
				background: none;
				font: inherit;
				color: var(--uui-color-selected);
				line-height: reset;
				padding: 0 var(--uui-size-space-2);
				margin: 0.5px 0 -0.5px;
				border: none;
				outline: none;
				width: 100%;
			}

			#tag-add-wrapper #tag-input:focus ~ uui-icon,
			#tag-add-wrapper #tag-input:not(:placeholder-shown) ~ uui-icon {
				display: none;
			}

			.tag uui-icon {
				margin-left: var(--uui-size-space-2);
			}

			.tag uui-icon:hover,
			.tag uui-icon:active {
				color: var(--uui-color-selected-contrast);
			}
		`,
	];

	@property({ type: String })
	group?: string;

	_items: string[] = [];
	@property({ type: Array })
	public set items(newTags: string[]) {
		this._items = newTags;
		super.value = this._items.join(',');
	}
	public get items(): string[] {
		return this._items;
	}

	@query('#tag-input')
	private _tagInput!: UUIInputElement;

	@query('#tag-add-wrapper')
	private _tagWrapper!: UUITagElement;

	public focus() {
		this._tagInput.focus();
	}

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
	}

	#onKeydown(e: KeyboardEvent) {
		//Prevent tab away if there is a input
		if (e.key === 'Tab' && (this._tagInput.value as string).trim().length) {
			e.preventDefault();
			this.#createTag();
			return;
		}
		if (e.key === 'Enter') {
			this.#createTag();
			return;
		}
		this.#inputError(false);
	}

	#createTag() {
		this.#inputError(false);
		const newTag = (this._tagInput.value as string).trim();
		if (!newTag) return;

		const tagExists = this.items.find((tag) => tag === newTag);
		if (tagExists) return this.#inputError(true);

		this.#inputError(false);
		this.items = [...this.items, newTag];
		this._tagInput.value = '';
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#inputError(error: boolean) {
		if (error) {
			this._tagWrapper.style.border = '1px solid var(--uui-color-danger)';
			this._tagInput.style.color = 'var(--uui-color-danger)';
			return;
		}
		this._tagWrapper.style.border = '';
		this._tagInput.style.color = '';
	}

	#delete(tag: string) {
		this.items.splice(
			this.items.findIndex((x) => x === tag),
			1
		);
		this.items = [...this.items];
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html` <div id="wrapper">
			${this.#renderTags()}
			<uui-tag look="outline" id="tag-add-wrapper" @click="${this.focus}">
				<input
					size="10"
					id="tag-input"
					aria-label="tag input"
					@keydown="${this.#onKeydown}"
					placeholder="Enter tag"
					@blur="${this.#createTag}" />
				<uui-icon id="icon-add" name="umb:add"></uui-icon>
			</uui-tag>
		</div>`;
	}

	#renderTags() {
		return html` ${this.items.map((tag) => {
			return html`
				<uui-tag class="tag">
					<span>${tag}</span>
					<uui-icon name="umb:wrong" @click="${() => this.#delete(tag)}"></uui-icon>
				</uui-tag>
			`;
		})}`;
	}
}

export default UmbInputTagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tags': UmbInputTagsElement;
	}
}
