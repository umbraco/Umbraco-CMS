import { LitElement, html, css, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeSVG } from 'lit/directives/unsafe-svg.js';
import { repeat } from 'lit/directives/repeat.js';
import type { ManagedIcon } from './types.js';
import './icon-related-input.element.js';
import '@umbraco-ui/uui';

@customElement('icon-editor')
export class IconEditorElement extends LitElement {
	@property({ type: Object })
	icon: ManagedIcon | null = null;

	@property({ type: Array })
	allIcons: ManagedIcon[] = [];

	@property({ type: Array })
	allGroups: string[] = [];

	@state()
	private _newKeyword = '';

	@state()
	private _newGroup = '';

	#dispatchUpdate(changes: Partial<ManagedIcon>) {
		if (!this.icon) return;
		this.dispatchEvent(
			new CustomEvent('icon-updated', {
				detail: { ...this.icon, ...changes, isDirty: true },
				bubbles: true,
				composed: true,
			}),
		);
	}

	#onNameChange(e: Event) {
		const input = e.target as HTMLInputElement;
		this.#dispatchUpdate({ name: input.value });
	}

	#addKeyword() {
		if (!this.icon || !this._newKeyword.trim()) return;
		const keyword = this._newKeyword.trim().toLowerCase();
		if (!this.icon.keywords.includes(keyword)) {
			this.#dispatchUpdate({ keywords: [...this.icon.keywords, keyword] });
		}
		this._newKeyword = '';
	}

	#removeKeyword(keyword: string) {
		if (!this.icon) return;
		this.#dispatchUpdate({ keywords: this.icon.keywords.filter((k) => k !== keyword) });
	}

	#addGroup() {
		if (!this.icon || !this._newGroup.trim()) return;
		const group = this._newGroup.trim().toLowerCase();
		if (!this.icon.groups.includes(group)) {
			this.#dispatchUpdate({ groups: [...this.icon.groups, group] });
		}
		this._newGroup = '';
	}

	#removeGroup(group: string) {
		if (!this.icon) return;
		this.#dispatchUpdate({ groups: this.icon.groups.filter((g) => g !== group) });
	}

	#onRelatedChanged(e: CustomEvent<string[]>) {
		this.#dispatchUpdate({ related: e.detail });
	}

	#onLegacyChange(e: Event) {
		const checkbox = e.target as HTMLInputElement;
		this.#dispatchUpdate({ legacy: checkbox.checked });
	}

	#onInternalChange(e: Event) {
		const checkbox = e.target as HTMLInputElement;
		this.#dispatchUpdate({ internal: checkbox.checked });
	}

	#onClose() {
		this.dispatchEvent(new CustomEvent('editor-close', { bubbles: true, composed: true }));
	}

	#onKeywordKeydown(e: KeyboardEvent) {
		if (e.key === 'Enter') {
			e.preventDefault();
			this.#addKeyword();
		}
	}

	#onGroupKeydown(e: KeyboardEvent) {
		if (e.key === 'Enter') {
			e.preventDefault();
			this.#addGroup();
		}
	}

	override render() {
		if (!this.icon) return nothing;

		return html`
			<div class="editor">
				<div class="header">
					<h3>Edit Icon</h3>
					<uui-button compact look="secondary" @click=${this.#onClose}>&times;</uui-button>
				</div>

				<div class="preview">
					${this.icon.svgMarkup ? unsafeSVG(this.icon.svgMarkup) : html`<span class="placeholder">No SVG</span>`}
				</div>

				<div class="field">
					<label>Name</label>
					<uui-input .value=${this.icon.name} @change=${this.#onNameChange}></uui-input>
				</div>

				<div class="field">
					<label>File</label>
					<span class="readonly">${this.icon.file}</span>
				</div>

				<div class="field">
					<label>Category</label>
					<span class="readonly">${this.icon.category}</span>
				</div>

				<div class="field">
					<label>Keywords</label>
					<div class="tag-list">
						${repeat(
							this.icon.keywords,
							(k) => k,
							(k) => html`
								<uui-tag look="secondary" class="tag">
									${k}
									<button class="tag-remove" @click=${() => this.#removeKeyword(k)}>&times;</button>
								</uui-tag>
							`,
						)}
					</div>
					${this.icon.lucideTags.length > 0 && this.icon.keywords.length === 0
						? html`
								<div class="suggestion">
									<small>Lucide tags available:</small>
									<div class="tag-list">
										${repeat(
											this.icon.lucideTags,
											(t) => t,
											(t) => html`
												<uui-tag
													look="outline"
													class="tag suggestion-tag"
													@click=${() => {
														if (!this.icon!.keywords.includes(t)) {
															this.#dispatchUpdate({ keywords: [...this.icon!.keywords, t] });
														}
													}}>
													+ ${t}
												</uui-tag>
											`,
										)}
									</div>
								</div>
							`
						: nothing}
					<div class="input-row">
						<uui-input
							placeholder="Add keyword..."
							.value=${this._newKeyword}
							@input=${(e: Event) => (this._newKeyword = (e.target as HTMLInputElement).value)}
							@keydown=${this.#onKeywordKeydown}></uui-input>
						<uui-button compact look="secondary" @click=${this.#addKeyword}>Add</uui-button>
					</div>
				</div>

				<div class="field">
					<label>Groups</label>
					<div class="tag-list">
						${repeat(
							this.icon.groups,
							(g) => g,
							(g) => html`
								<uui-tag look="secondary" class="tag">
									${g}
									<button class="tag-remove" @click=${() => this.#removeGroup(g)}>&times;</button>
								</uui-tag>
							`,
						)}
					</div>
					${this.allGroups.length > 0
						? html`
								<div class="suggestion">
									<small>Existing groups:</small>
									<div class="tag-list">
										${repeat(
											this.allGroups.filter((g) => !this.icon!.groups.includes(g)),
											(g) => g,
											(g) => html`
												<uui-tag
													look="outline"
													class="tag suggestion-tag"
													@click=${() => {
														if (!this.icon!.groups.includes(g)) {
															this.#dispatchUpdate({ groups: [...this.icon!.groups, g] });
														}
													}}>
													+ ${g}
												</uui-tag>
											`,
										)}
									</div>
								</div>
							`
						: nothing}
					<div class="input-row">
						<uui-input
							placeholder="Add group..."
							.value=${this._newGroup}
							@input=${(e: Event) => (this._newGroup = (e.target as HTMLInputElement).value)}
							@keydown=${this.#onGroupKeydown}></uui-input>
						<uui-button compact look="secondary" @click=${this.#addGroup}>Add</uui-button>
					</div>
				</div>

				<div class="field">
					<label>Related Icons</label>
					<icon-related-input
						.selectedNames=${this.icon.related}
						.allIcons=${this.allIcons}
						@related-changed=${this.#onRelatedChanged}></icon-related-input>
				</div>

				<div class="field">
					<label>Flags</label>
					<div class="flags">
						<uui-checkbox .checked=${this.icon.legacy} @change=${this.#onLegacyChange}>Legacy</uui-checkbox>
						<uui-checkbox .checked=${this.icon.internal} @change=${this.#onInternalChange}>Internal</uui-checkbox>
					</div>
				</div>
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: block;
			height: 100%;
		}

		.editor {
			padding: 16px;
			height: 100%;
			overflow-y: auto;
			box-sizing: border-box;
		}

		.header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 16px;
		}

		.header h3 {
			margin: 0;
		}

		.preview {
			width: 64px;
			height: 64px;
			margin: 0 auto 16px;
			color: var(--uui-color-text, #1b264f);
		}

		.preview svg {
			width: 100%;
			height: 100%;
		}

		.placeholder {
			display: flex;
			align-items: center;
			justify-content: center;
			width: 100%;
			height: 100%;
			opacity: 0.3;
			font-size: 12px;
		}

		.field {
			margin-bottom: 16px;
		}

		.field label {
			display: block;
			font-size: 12px;
			font-weight: 600;
			margin-bottom: 4px;
			text-transform: uppercase;
			opacity: 0.7;
		}

		.readonly {
			font-size: 13px;
			opacity: 0.6;
		}

		uui-input {
			width: 100%;
		}

		.tag-list {
			display: flex;
			flex-wrap: wrap;
			gap: 4px;
			margin-bottom: 6px;
		}

		.tag {
			position: relative;
			cursor: default;
		}

		.tag-remove {
			background: none;
			border: none;
			cursor: pointer;
			font-size: 14px;
			padding: 0 0 0 4px;
			color: inherit;
			opacity: 0.6;
		}

		.tag-remove:hover {
			opacity: 1;
		}

		.suggestion {
			margin-bottom: 6px;
		}

		.suggestion small {
			display: block;
			font-size: 11px;
			opacity: 0.5;
			margin-bottom: 4px;
		}

		.suggestion-tag {
			cursor: pointer;
			font-size: 11px;
		}

		.suggestion-tag:hover {
			opacity: 0.8;
		}

		.input-row {
			display: flex;
			gap: 4px;
			align-items: center;
		}

		.input-row uui-input {
			flex: 1;
		}

		.flags {
			display: flex;
			gap: 16px;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'icon-editor': IconEditorElement;
	}
}
