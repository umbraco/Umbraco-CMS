import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-search')
export class UmbSearchElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
				width: 100%;
				height: 100%;
				background-color: var(--uui-color-surface-alt);
				box-sizing: border-box;
				color: var(--uui-color-text);
				font-size: 1rem;
			}
			input {
				all: unset;
				height: 100%;
				width: 100%;
			}
			#search-icon,
			#close-icon {
				display: flex;
				align-items: center;
				justify-content: center;
				aspect-ratio: 1;
				height: 100%;
			}
			#close-icon > div {
				background: var(--uui-color-surface-alt);
				border: 1px solid var(--uui-color-border);
				padding: 3px 6px 4px 6px;
				line-height: 1;
				border-radius: 3px;
				color: var(--uui-color-text-alt);
				font-weight: 800;
				font-size: 12px;
			}
			#close-icon > div:hover {
				border-color: var(--uui-color-focus);
				color: var(--uui-color-focus);
				cursor: pointer;
			}
			#top {
				background-color: var(--uui-color-surface);
				display: flex;
				height: 48px;
				border-bottom: 1px solid var(--uui-color-border);
			}
			#main {
				display: flex;
				flex-direction: column;
				padding: 0 32px 16px 32px;
			}
			.group {
				margin-top: var(--uui-size-space-4);
			}
			.group-name {
				font-weight: 600;
				margin-bottom: var(--uui-size-space-1);
			}
			.results {
				display: flex;
				flex-direction: column;
				gap: 8px;
			}
			.result {
				background: var(--uui-color-surface);
				border: 1px solid var(--uui-color-border);
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				cursor: pointer;
				justify-content: space-between;
				display: flex;
			}
			.result:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			a {
				text-decoration: none;
				color: inherit;
			}
		`,
	];

	connectedCallback() {
		super.connectedCallback();

		requestAnimationFrame(() => {
			this.shadowRoot?.querySelector('input')?.focus();
		});
	}

	render() {
		return html`
			<div id="top">
                <div id="search-icon">
                    <uui-icon name="search"></uui-icon>
                </div>
				<input type="text" placeholder="Search..." autocomplete="off"></input>
                <div id="close-icon">
                    <div>esc</div>
                </div>
			</div>
			<div id="main">
                <div class="group">
                    <div class="group-name">Document Types</div>
                    <div class="results">
                        <a href="#" class="result">
                            Article Controls
                            <span style="opacity: 0.5; font-weight: 100">></span>
                        </a>
                        <a href="#" class="result">
                            Article
                        </a>
                    </div>
                </div>
                <div class="group">
                    <div class="group-name">Media Types</div>
                    <div class="results">
                        <a href="#" class="result">
                            Article
                        </a>
                    </div>
                </div>
            </div>
		`;
	}
}

export default UmbSearchElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search': UmbSearchElement;
	}
}
