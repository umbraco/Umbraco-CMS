import { LitElement, html, css, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { unsafeSVG } from 'lit/directives/unsafe-svg.js';
import type { ManagedIcon } from './types.js';
import '@umbraco-ui/uui';

@customElement('icon-card')
export class IconCardElement extends LitElement {
	@property({ type: Object })
	icon!: ManagedIcon;

	@property({ type: String })
	mode: 'picked' | 'unpicked' = 'picked';

	#onAdd(e: Event) {
		e.stopPropagation();
		this.dispatchEvent(new CustomEvent('icon-add', { detail: this.icon, bubbles: true, composed: true }));
	}

	#onSelect() {
		this.dispatchEvent(new CustomEvent('icon-select', { detail: this.icon, bubbles: true, composed: true }));
	}

	override render() {
		if (!this.icon) return nothing;

		const hasMetadata = this.icon.keywords.length > 0 || this.icon.groups.length > 0 || this.icon.related.length > 0;

		return html`
			<div
				class="card ${this.icon.isNew ? 'new' : ''} ${this.icon.isDirty ? 'dirty' : ''}"
				@click=${this.#onSelect}
				tabindex="0"
				@keydown=${(e: KeyboardEvent) => e.key === 'Enter' && this.#onSelect()}>
				<div class="icon-preview">
					${this.icon.svgMarkup ? unsafeSVG(this.icon.svgMarkup) : html`<span class="placeholder">?</span>`}
				</div>
				<div class="icon-name" title=${this.icon.name}>${this.icon.name}</div>
				<div class="icon-file" title=${this.icon.file}>${this.icon.file}</div>
				${this.mode === 'picked' && hasMetadata ? html`<div class="metadata-dot" title="Has metadata"></div>` : nothing}
				${this.icon.isNew ? html`<div class="badge new-badge">NEW</div>` : nothing}
				${this.icon.isDirty ? html`<div class="badge dirty-badge">EDITED</div>` : nothing}
				${this.mode === 'unpicked'
					? html`<uui-button class="add-btn" look="primary" compact @click=${this.#onAdd}>+</uui-button>`
					: nothing}
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: block;
		}

		.card {
			position: relative;
			display: flex;
			flex-direction: column;
			align-items: center;
			padding: 12px 8px 8px;
			border: 1px solid var(--uui-color-border, #d8d7d9);
			border-radius: 6px;
			background: var(--uui-color-surface, #fff);
			cursor: pointer;
			transition: border-color 0.15s, box-shadow 0.15s;
			min-height: 100px;
		}

		.card:hover {
			border-color: var(--uui-color-interactive, #3544b1);
			box-shadow: 0 0 0 1px var(--uui-color-interactive, #3544b1);
		}

		.card:focus-visible {
			outline: 2px solid var(--uui-color-interactive, #3544b1);
			outline-offset: 2px;
		}

		.card.new {
			border-color: var(--uui-color-positive, #2bc37c);
		}

		.card.dirty {
			border-color: var(--uui-color-warning, #f5c142);
		}

		.icon-preview {
			width: 32px;
			height: 32px;
			display: flex;
			align-items: center;
			justify-content: center;
			color: var(--uui-color-text, #1b264f);
		}

		.icon-preview svg {
			width: 100%;
			height: 100%;
		}

		.placeholder {
			font-size: 20px;
			opacity: 0.3;
		}

		.icon-name {
			margin-top: 6px;
			font-size: 10px;
			font-weight: 600;
			text-align: center;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			width: 100%;
		}

		.icon-file {
			font-size: 9px;
			opacity: 0.5;
			text-align: center;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			width: 100%;
		}

		.metadata-dot {
			position: absolute;
			top: 4px;
			right: 4px;
			width: 6px;
			height: 6px;
			border-radius: 50%;
			background: var(--uui-color-interactive, #3544b1);
		}

		.badge {
			position: absolute;
			top: 4px;
			left: 4px;
			font-size: 8px;
			font-weight: 700;
			padding: 1px 4px;
			border-radius: 3px;
			text-transform: uppercase;
		}

		.new-badge {
			background: var(--uui-color-positive, #2bc37c);
			color: white;
		}

		.dirty-badge {
			background: var(--uui-color-warning, #f5c142);
			color: #1b264f;
		}

		.add-btn {
			position: absolute;
			top: 4px;
			right: 4px;
			--uui-button-height: 24px;
			font-size: 16px;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'icon-card': IconCardElement;
	}
}
