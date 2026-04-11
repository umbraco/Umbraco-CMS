import { LitElement, html, css, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeSVG } from 'lit/directives/unsafe-svg.js';
import { repeat } from 'lit/directives/repeat.js';
import type { ManagedIcon } from './types.js';
import '@umbraco-ui/uui';

@customElement('icon-related-input')
export class IconRelatedInputElement extends LitElement {
	@property({ type: Array })
	selectedNames: string[] = [];

	@property({ type: Array })
	allIcons: ManagedIcon[] = [];

	@state()
	private _query = '';

	@state()
	private _showDropdown = false;

	get #filteredIcons(): ManagedIcon[] {
		if (!this._query) return [];
		const q = this._query.toLowerCase();
		return this.allIcons
			.filter((icon) => icon.name.toLowerCase().includes(q) && !this.selectedNames.includes(icon.name))
			.slice(0, 20);
	}

	get #selectedIcons(): ManagedIcon[] {
		return this.selectedNames
			.map((name) => this.allIcons.find((i) => i.name === name))
			.filter((i): i is ManagedIcon => !!i);
	}

	#onInput(e: Event) {
		const input = e.target as HTMLInputElement;
		this._query = input.value;
		this._showDropdown = this._query.length > 0;
	}

	#addRelated(icon: ManagedIcon) {
		if (!this.selectedNames.includes(icon.name)) {
			const updated = [...this.selectedNames, icon.name];
			this.#dispatchChange(updated);
		}
		this._query = '';
		this._showDropdown = false;
	}

	#removeRelated(name: string) {
		const updated = this.selectedNames.filter((n) => n !== name);
		this.#dispatchChange(updated);
	}

	#dispatchChange(names: string[]) {
		this.dispatchEvent(new CustomEvent('related-changed', { detail: names, bubbles: true, composed: true }));
	}

	#onBlur() {
		// Delay to allow click on dropdown item
		setTimeout(() => {
			this._showDropdown = false;
		}, 200);
	}

	override render() {
		return html`
			<div class="selected-list">
				${repeat(
					this.#selectedIcons,
					(icon) => icon.name,
					(icon) => html`
						<div class="selected-item">
							<span class="mini-icon">${icon.svgMarkup ? unsafeSVG(icon.svgMarkup) : nothing}</span>
							<span class="item-name">${icon.name}</span>
							<button class="remove-btn" @click=${() => this.#removeRelated(icon.name)}>&times;</button>
						</div>
					`,
				)}
			</div>
			<div class="input-wrapper">
				<uui-input
					placeholder="Search icons to add..."
					.value=${this._query}
					@input=${this.#onInput}
					@focus=${() => this._query && (this._showDropdown = true)}
					@blur=${this.#onBlur}></uui-input>
				${this._showDropdown && this.#filteredIcons.length > 0
					? html`
							<div class="dropdown">
								${repeat(
									this.#filteredIcons,
									(icon) => icon.name,
									(icon) => html`
										<div class="dropdown-item" @mousedown=${() => this.#addRelated(icon)}>
											<span class="mini-icon">${icon.svgMarkup ? unsafeSVG(icon.svgMarkup) : nothing}</span>
											<span>${icon.name}</span>
										</div>
									`,
								)}
							</div>
						`
					: nothing}
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: block;
		}

		.selected-list {
			display: flex;
			flex-wrap: wrap;
			gap: 4px;
			margin-bottom: 8px;
		}

		.selected-item {
			display: inline-flex;
			align-items: center;
			gap: 4px;
			padding: 2px 6px;
			border-radius: 4px;
			background: var(--uui-color-surface-alt, #f3f3f5);
			border: 1px solid var(--uui-color-border, #d8d7d9);
			font-size: 12px;
		}

		.mini-icon {
			width: 14px;
			height: 14px;
			display: inline-flex;
			align-items: center;
		}

		.mini-icon svg {
			width: 100%;
			height: 100%;
		}

		.item-name {
			max-width: 120px;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
		}

		.remove-btn {
			background: none;
			border: none;
			cursor: pointer;
			font-size: 14px;
			padding: 0 2px;
			color: var(--uui-color-text, #1b264f);
			opacity: 0.6;
		}

		.remove-btn:hover {
			opacity: 1;
		}

		.input-wrapper {
			position: relative;
		}

		uui-input {
			width: 100%;
		}

		.dropdown {
			position: absolute;
			top: 100%;
			left: 0;
			right: 0;
			max-height: 200px;
			overflow-y: auto;
			background: var(--uui-color-surface, #fff);
			border: 1px solid var(--uui-color-border, #d8d7d9);
			border-radius: 4px;
			box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
			z-index: 100;
		}

		.dropdown-item {
			display: flex;
			align-items: center;
			gap: 8px;
			padding: 6px 10px;
			cursor: pointer;
			font-size: 12px;
		}

		.dropdown-item:hover {
			background: var(--uui-color-surface-alt, #f3f3f5);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'icon-related-input': IconRelatedInputElement;
	}
}
