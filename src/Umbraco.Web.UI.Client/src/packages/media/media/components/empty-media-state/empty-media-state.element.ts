import { LitElement, customElement, html, css } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-empty-media-state')
export class UmbEmptyMediaStateElement extends LitElement {
	#onBrowse() {
		// Dispatch a custom event so the parent knows the button was clicked
		this.dispatchEvent(new CustomEvent('browse', { bubbles: false, composed: false }));
	}

	override render() {
		return html`
			<uui-icon name="icon-picture"></uui-icon>
			<p>Drag and drop your media files here</p>
			<p>or</p>
			<uui-button look="primary" label="Browse files" @click=${this.#onBrowse}> Browse files </uui-button>
		`;
	}

	static override styles = [
		css`
			:host {
				cursor: pointer;
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				background-color: var(--uui-palette-sand);
				border: 1px dashed var(--uui-color-border-standalone, --uui-palette-grey-light);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-default);
				opacity: 0;
				animation: fadeInEmptyState 0.2s ease-in forwards 0.15s;
			}

			@keyframes fadeInEmptyState {
				to {
					opacity: 1;
				}
			}

			uui-icon {
				font-size: clamp(1rem, 2.5vw, 3rem);
				margin-bottom: var(--uui-size-space-4);
				color: var(--uui-color-default);
			}

			uui-button {
				z-index: 1000;
				position: relative;
				pointer-events: auto; /* Ensures the button is always clickable even if the host ignores pointers */
			}
		`,
	];
}

export default UmbEmptyMediaStateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-empty-media-state': UmbEmptyMediaStateElement;
	}
}
