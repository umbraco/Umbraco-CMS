import { LogLevelModel, LogMessagePropertyModel } from '@umbraco-cms/backend-api';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

@customElement('umb-log-viewer-message')
export class UmbLogViewerMessageElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host > details {
				border-top: 1px solid var(--uui-color-border);
			}

			:host(:last-child) > details {
				border-bottom: 1px solid var(--uui-color-border);
			}

			summary {
				display: flex;
			}

			summary:hover,
			ul {
				background-color: var(--uui-color-background);
			}

			ul {
				margin: 0;
				padding: 0;
				list-style: none;
			}

			li {
				padding: 10px 20px;
				display: flex;
				border-top: 1px solid var(--uui-color-border);
			}

			summary > div {
				box-sizing: border-box;
				padding: 10px 20px;
				display: flex;
				align-items: center;
			}

			#timestamp {
				flex: 1 0 14ch;
			}

			#level,
			#machine {
				flex: 1 0 14ch;
			}

			#message {
				flex: 6 0 14ch;
			}

			.property-name {
				font-weight: 600;
				flex: 1 1 20ch;
			}

			.property-value {
				flex: 3 0 20ch;
			}

			#search-menu {
				margin: 0;
				padding: 0;
				margin-top: var(--uui-size-space-3);
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-2);
				max-width: 20%;
			}

			#search-menu > li {
				padding: 0;
			}

			.search-item {
				width: 100%;
			}
		`,
	];

	@property()
	timestamp = '';

	@state()
	date?: Date;

	@property()
	level: LogLevelModel | '' = '';

	@property()
	messageTemplate = '';

	@property()
	renderedMessage = '';

	@property({ attribute: false })
	properties: Array<LogMessagePropertyModel> = [];

	@property()
	exception = '';

	willUpdate(changedProperties: Map<string | number | symbol, unknown>) {
		if (changedProperties.has('timestamp')) {
			this.date = new Date(this.timestamp);
		}
	}

	render() {
		return html`
			<details>
				<summary>
					<div id="timestamp">${this.date?.toLocaleString()}</div>
					<div id="level">
						<umb-log-viewer-level-tag .level=${this.level ? this.level : 'Information'}></umb-log-viewer-level-tag>
					</div>
					<div id="machine">${this.properties.find((property) => property.name === 'MachineName')?.value}</div>
					<div id="message">${this.renderedMessage}</div>
				</summary>
				<ul>
					<li>
						<div class="property-name">Timestamp</div>
						<div class="property-value">${this.date?.toLocaleString()}</div>
					</li>
					<li>
						<div class="property-name">@MessageTemplate</div>
						<div class="property-value">${this.messageTemplate}</div>
					</li>
					${this.properties.map(
						(property) =>
							html`<li>
								<div class="property-name">${property.name}:</div>
								<div class="property-value">${property.value}</div>
							</li>`
					)}
				</ul>
				<umb-button-with-dropdown look="secondary" placement="bottom-start">
					<uui-icon name="umb:search"></uui-icon>Search
					<ul id="search-menu" slot="dropdown">
						<li>
							<uui-button
								class="search-item"
								href="https://www.google.com/search?q=${this.renderedMessage}"
								target="_blank"
								>Google</uui-button
							>
						</li>
					</ul>
				</umb-button-with-dropdown>
			</details>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-message': UmbLogViewerMessageElement;
	}
}
