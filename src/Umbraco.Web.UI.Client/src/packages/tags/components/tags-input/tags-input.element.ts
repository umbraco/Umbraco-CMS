import { UmbTagRepository } from '../../repository/tag.repository.js';
import {
	css,
	customElement,
	html,
	nothing,
	property,
	query,
	queryAll,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { TagResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UUIInputElement, UUIInputEvent, UUITagElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-tags-input')
export class UmbTagsInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ type: String })
	group?: string;

	@property({ type: String })
	culture?: string | null;

	@property({ type: Array })
	public set items(newTags: string[]) {
		const newItems = newTags.filter((x) => x !== '');
		this.#items = newItems;
		super.value = this.#items.join(',');
	}
	public get items(): string[] {
		return this.#items;
	}
	#items: string[] = [];

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _matches: Array<TagResponseModel> = [];

	@state()
	private _currentInput = '';

	@query('#main-tag')
	private _mainTag!: UUITagElement;

	@query('#tag-input')
	private _tagInput!: UUIInputElement;

	@query('#input-width-tracker')
	private _widthTracker!: HTMLElement;

	@queryAll('.options')
	private _optionCollection?: HTMLCollectionOf<HTMLInputElement>;

	@queryAll('.tag')
	private _tagEls?: NodeListOf<HTMLElement>;

	#repository = new UmbTagRepository(this);

	public override focus() {
		this._tagInput.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	async #getExistingTags(query: string) {
		if (!this.group || !query) return;
		const { data } = await this.#repository.queryTags(this.group, this.culture ?? null, query);
		if (!data) return;
		this._matches = data.items;
	}

	#onInputKeydown(e: KeyboardEvent) {
		const inputLength = (this._tagInput.value as string).trim().length;

		//Prevent tab away if there is a text in the input.
		if (e.key === 'Tab' && inputLength && !this._matches.length) {
			e.preventDefault();
			this.#createTag();
			return;
		}

		//If the input is empty we can navigate out of it using tab
		if (e.key === 'Tab' && !inputLength) {
			return;
		}

		//Create a new tag when enter to the input
		if (e.key === 'Enter') {
			this.#createTag();
			return;
		}

		//This one to show option collection if there is any
		if (e.key === 'ArrowDown') {
			e.preventDefault();
			this._currentInput = this._optionCollection?.item(0)?.value ?? this._currentInput;
			this._optionCollection?.item(0)?.focus();
			return;
		}
		this.#inputError(false);
	}

	#focusTag(index: number) {
		const tag = this._tagEls?.[index];
		if (!tag) return;

		// Find the current element with the class .tab and tabindex=0 (will be the previous tag)
		const active = this.renderRoot.querySelector<HTMLElement>('.tag[tabindex="0"]');

		// Return it is tabindex to -1
		active?.setAttribute('tabindex', '-1');

		// Set the tabindex to 0 in the current target
		tag.setAttribute('tabindex', '0');

		tag.focus();
	}

	#onTagsWrapperKeydown(e: KeyboardEvent) {
		if ((e.key === 'Enter' || e.key === 'ArrowDown') && this.items.length) {
			e.preventDefault();
			this.#focusTag(0);
		}
	}

	#onTagKeydown(e: KeyboardEvent, idx: number) {
		if (e.key === 'ArrowRight') {
			e.preventDefault();
			if (idx < this.items.length - 1) {
				this.#focusTag(idx + 1);
			}
		}

		if (e.key === 'ArrowLeft') {
			e.preventDefault();
			if (idx > 0) {
				this.#focusTag(idx - 1);
			}
		}

		if (e.key === 'Backspace' || e.key === 'Delete') {
			e.preventDefault();
			if (this.#items.length - 1 === idx) {
				this.#focusTag(idx - 1);
			}
			this.#delete(this.#items[idx]);
			this.#focusTag(idx + 1);
		}
	}

	#onInput(e: UUIInputEvent) {
		this._currentInput = e.target.value as string;
		if (!this._currentInput || !this._currentInput.length) {
			this._matches = [];
		} else {
			this.#getExistingTags(this._currentInput);
		}
	}

	protected override updated(): void {
		this._mainTag.style.width = `${this._widthTracker.offsetWidth - 4}px`;
	}

	#onBlur() {
		if (this._matches.length) return;
		else this.#createTag();
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
		this._currentInput = '';
		this.dispatchEvent(new UmbChangeEvent());
	}

	#inputError(error: boolean) {
		if (error) {
			this._mainTag.style.border = '1px solid var(--uui-color-danger)';
			this._tagInput.style.color = 'var(--uui-color-danger)';
			return;
		}
		this._mainTag.style.border = '';
		this._tagInput.style.color = '';
	}

	#delete(tag: string) {
		const currentItems = [...this.items];
		const index = currentItems.findIndex((x) => x === tag);
		currentItems.splice(index, 1);
		if (currentItems.length) {
			this.items = currentItems;
		} else {
			this.items = [];
		}
		this.dispatchEvent(new UmbChangeEvent());
	}

	/** Dropdown */

	#optionClick(index: number) {
		this._tagInput.value = this._optionCollection?.item(index)?.value ?? '';
		this.#createTag();
		this.focus();
		return;
	}

	#optionKeydown(e: KeyboardEvent, index: number) {
		if (e.key === 'Enter' || e.key === 'Tab') {
			e.preventDefault();
			this._currentInput = this._optionCollection?.item(index)?.value ?? '';
			this.#createTag();
			this.focus();
			return;
		}

		if (e.key === 'ArrowDown') {
			e.preventDefault();
			if (!this._optionCollection?.item(index + 1)) return;
			this._optionCollection?.item(index + 1)?.focus();
			this._currentInput = this._optionCollection?.item(index + 1)?.value ?? '';
			return;
		}

		if (e.key === 'ArrowUp') {
			e.preventDefault();
			if (!this._optionCollection?.item(index - 1)) return;
			this._optionCollection?.item(index - 1)?.focus();
			this._currentInput = this._optionCollection?.item(index - 1)?.value ?? '';
		}

		if (e.key === 'Backspace') {
			this.focus();
		}
	}

	/** Render */

	override render() {
		return html`
			<div id="wrapper">
				${this.#renderTags()}
				<span id="main-tag-wrapper">
					<uui-tag id="input-width-tracker" aria-hidden="true" style="visibility:hidden;opacity:0;position:absolute;">
						${this._currentInput}
					</uui-tag>
					${this.#renderAddButton()}
				</span>
			</div>
		`;
	}

	#renderTags() {
		return html`
			<div id="tags" tabindex="0" @keydown=${this.#onTagsWrapperKeydown}>
				${repeat(
					this.items,
					(tag) => tag,
					(tag, index) => html`
						<uui-tag class="tag" @keydown=${(e: KeyboardEvent) => this.#onTagKeydown(e, index)}>
							<span>${tag}</span>
							${this.#renderRemoveButton(tag)}
						</uui-tag>
					`,
				)}
			</div>
		`;
	}

	#renderTagOptions() {
		if (!this._matches.length) return nothing;
		const matchfilter = this._matches.filter((tag) => tag.text !== this.#items.find((x) => x === tag.text));
		if (!matchfilter.length) return;
		return html`
			<div id="matchlist">
				${repeat(
					matchfilter.slice(0, 5),
					(tag: TagResponseModel) => tag.id,
					(tag: TagResponseModel, index: number) => {
						return html`<input
								class="options"
								id="tag-${tag.id}"
								type="radio"
								name="${tag.group ?? ''}"
								@click="${() => this.#optionClick(index)}"
								@keydown="${(e: KeyboardEvent) => this.#optionKeydown(e, index)}"
								value="${tag.text ?? ''}"
								?readonly=${this.readonly} />
							<label for="tag-${tag.id}"> ${tag.text} </label>`;
					},
				)}
			</div>
		`;
	}

	#renderAddButton() {
		if (this.readonly) return nothing;
		return html`
			<uui-tag look="outline" id="main-tag" @click="${this.focus}" slot="trigger">
				<input
					id="tag-input"
					aria-label="tag input"
					autocomplete="off"
					placeholder="Enter tag"
					.value="${this._currentInput ?? undefined}"
					@keydown="${this.#onInputKeydown}"
					@input="${this.#onInput}"
					@blur="${this.#onBlur}" />
				<uui-icon id="icon-add" name="icon-add"></uui-icon>
				${this.#renderTagOptions()}
			</uui-tag>
		`;
	}

	#renderRemoveButton(tag: string) {
		if (this.readonly) return nothing;
		return html`<uui-icon name="icon-wrong" @click="${() => this.#delete(tag)}"></uui-icon>`;
	}

	static override styles = [
		css`
			#wrapper {
				box-sizing: border-box;
				display: flex;
				gap: var(--uui-size-space-2);
				flex-wrap: wrap;
				align-items: center;
				padding: var(--uui-size-space-2);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-input-background-color, var(--uui-color-surface));
				flex: 1;
				min-height: 40px;
			}

			#main-tag-wrapper {
				position: relative;
			}

			/** Tags */
			#tags {
				display: flex;
				gap: var(--uui-size-space-2);
				flex-wrap: wrap;
				border-radius: var(--uui-size-1);

				&:focus {
					outline: var(--uui-size-1) solid var(--uui-color-focus);
					outline-offset: var(--uui-size-1);
				}
			}

			uui-tag {
				position: relative;
				max-width: 200px;
			}

			uui-tag uui-icon {
				cursor: pointer;
				min-width: 12.8px !important;
			}

			uui-tag span {
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}

			/** Existing tags */
			.tag {
				&:focus {
					outline: var(--uui-size-1) solid var(--uui-color-focus);
				}

				uui-icon {
					margin-left: var(--uui-size-space-2);

					&:hover,
					&:active {
						color: var(--uui-color-selected-contrast);
					}
				}
			}

			/** Main tag */

			#main-tag {
				padding: 3px;
				background-color: var(--uui-color-selected-contrast);
				min-width: 20px;
				position: relative;
				border-radius: var(--uui-size-5, 12px);
			}

			#main-tag uui-icon {
				position: absolute;
				top: 50%;
				left: 50%;
				transform: translate(-50%, -50%);
			}

			#main-tag:hover uui-icon,
			#main-tag:active uui-icon {
				color: var(--uui-color-selected);
			}

			#main-tag #tag-input:focus ~ uui-icon,
			#main-tag #tag-input:not(:placeholder-shown) ~ uui-icon {
				display: none;
			}

			#main-tag:has(*:hover),
			#main-tag:has(*:active),
			#main-tag:has(*:focus) {
				border: 1px solid var(--uui-color-selected-emphasis);
			}

			#main-tag:has(#tag-input:not(:focus)):hover {
				cursor: pointer;
				border: 1px solid var(--uui-color-selected-emphasis);
			}

			#main-tag:not(:focus-within) #tag-input:placeholder-shown {
				opacity: 0;
			}

			#main-tag:has(#tag-input:focus),
			#main-tag:has(#tag-input:not(:placeholder-shown)) {
				min-width: 65px;
			}

			#main-tag #tag-input {
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

			/** Dropdown matchlist */

			#matchlist input[type='radio'] {
				-webkit-appearance: none;
				appearance: none;
				/* For iOS < 15 to remove gradient background */
				background-color: transparent;
				/* Not removed via appearance */
				margin: 0;
			}

			uui-tag:focus-within #matchlist {
				display: flex;
			}

			#matchlist {
				display: flex;
				flex-direction: column;
				background-color: var(--uui-color-surface);
				position: absolute;
				width: 150px;
				left: 0;
				top: var(--uui-size-space-6);
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				z-index: 10;
			}

			#matchlist label {
				display: none;
				cursor: pointer;
				box-sizing: border-box;
				display: block;
				width: 100%;
				background: none;
				border: none;
				text-align: left;
				padding: 10px 12px;

				/** Overflow */
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}

			#matchlist label:hover,
			#matchlist label:focus,
			#matchlist label:focus-within,
			#matchlist input[type='radio']:focus + label {
				display: block;
				background-color: var(--uui-color-focus);
				color: var(--uui-color-selected-contrast);
			}
		`,
	];
}

export default UmbTagsInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tags-input': UmbTagsInputElement;
	}
}
