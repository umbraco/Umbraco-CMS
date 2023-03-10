import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UUIInputElement } from '@umbraco-ui/uui';
import { customElement, property, query } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-input-tags')
export class UmbInputTagsElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			#tags-wrapper {
				margin-top: var(--uui-size-space-4);
				display: flex;
				gap: var(--uui-size-space-2);
				flex-wrap: wrap;
			}

			uui-tag uui-icon {
				cursor: pointer;
				margin-left: var(--uui-size-space-2);
			}

			uui-tag uui-icon:hover,
			uui-tag uui-icon:active {
				color: var(--uui-color-selected-contrast);
			}
		`,
	];

	@property({ type: String })
	group?: string;

	_tags: string[] = [];
	@property({ type: Array })
	public set tags(newTags: string[]) {
		this._tags = newTags;
		super.value = this._tags.join(',');
	}
	public get tags(): string[] {
		return this._tags;
	}

	@query('#tag-input')
	private _tagInput!: UUIInputElement;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
	}

	#onKeypress(e: KeyboardEvent) {
		if (e.key !== 'Enter') return;
		const newTag = (this._tagInput.value as string).trim();

		if (!newTag) return this.#inputError();

		const tagExists = this.tags.find((tag) => tag === newTag);

		if (tagExists) return this.#inputError();

		this._tagInput.error = false;
		this.tags = [...this.tags, newTag];
		this._tagInput.value = '';
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#inputError() {
		this._tagInput.error = true;
	}

	#delete(tag: string) {
		this.tags.splice(
			this.tags.findIndex((x) => x === tag),
			1
		);
		this.tags = [...this.tags];
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html` <uui-input
				id="tag-input"
				label="tag input"
				@keypress=${this.#onKeypress}
				placeholder="Press enter after each tag"></uui-input>
			${this.#renderTags()}`;
	}

	#renderTags() {
		if (!this.tags.length) return nothing;
		return html`<div id="tags-wrapper">
			${this.tags.map((tag) => {
				return html`
					<uui-tag>
						${tag}
						<uui-icon name="umb:wrong" @click="${() => this.#delete(tag)}"></uui-icon>
					</uui-tag>
				`;
			})}
		</div>`;
	}
}

export default UmbInputTagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tags': UmbInputTagsElement;
	}
}
