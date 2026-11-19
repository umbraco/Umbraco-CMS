import { customElement, html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-empty-media-state')
export class UmbEmptyMediaStateElement extends UmbLitElement {
	#dragCounter = 0;

	// Event handlers as arrow functions to maintain consistent references
	#handleDragEnter = () => {
		this.#dragCounter++;
		this.toggleAttribute('dragging', true);
	};

	#handleDragLeave = () => {
		this.#dragCounter--;
		if (this.#dragCounter <= 0) {
			this.toggleAttribute('dragging', false);
			this.#dragCounter = 0;
		}
	};

	override connectedCallback() {
		super.connectedCallback();
		document.addEventListener('dragenter', this.#handleDragEnter);
		document.addEventListener('dragleave', this.#handleDragLeave);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this.#handleDragEnter);
		document.removeEventListener('dragleave', this.#handleDragLeave);
	}

	#onBrowse() {
		// Dispatch a custom event so the parent knows the button was clicked
		this.dispatchEvent(new CustomEvent('browse', { bubbles: false, composed: false }));
	}

	override render() {
		return html`
			<uui-icon name="icon-image-up"></uui-icon>
			<p>${this.localize.htmlString('#media_dropFilesOr')}</p>
			<uui-button
				look="outline"
				label=${this.localize.term('media_browseFilesAction')}
				@click=${this.#onBrowse}></uui-button>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				border: 1px dashed var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				opacity: 0;
				animation: fadeInEmptyState 840ms forwards 640ms;
			}

			@keyframes fadeInEmptyState {
				to {
					opacity: 1;
				}
			}

			:host([dragging]) {
				visibility: hidden;
			}

			p {
				text-align: center;
				line-height: 1.5;
			}

			uui-icon {
				font-size: 4em;
				color: var(--uui-color-border-standalone);
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
