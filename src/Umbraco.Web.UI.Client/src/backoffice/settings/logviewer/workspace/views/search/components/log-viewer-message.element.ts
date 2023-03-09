import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, PropertyValueMap } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { LogLevelModel, LogMessagePropertyModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

//TODO: check how to display EventId field in the message properties
@customElement('umb-log-viewer-message')
export class UmbLogViewerMessageElement extends UmbLitElement {
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

			details[open] {
				margin-bottom: var(--uui-size-space-3);
			}

			summary:hover,
			#properties-list {
				background-color: var(--uui-color-background);
			}

			#properties-list {
				margin: 0;
				padding: 0;
				list-style: none;
				margin-bottom: var(--uui-size-space-3);
			}

			.property {
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

			.property-name,
			.property-value {
				display: flex;
				align-items: center;
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
				box-shadow: var(--uui-shadow-depth-3);
				max-width: 25%;
			}

			#search-menu > li {
				padding: 0;
			}

			.search-item {
				width: 100%;
			}

			pre {
				background-color: var(--uui-color-background);
				border-top: 1px solid #d8d7d9;
				border-left: 4px solid #d42054;
				color: #303033;
				display: block;
				font-family: Lato, Helvetica Neue, Helvetica, Arial, sans-serif;
				line-height: 20px;
				margin: 0;
				overflow-x: auto;
				padding: 9.5px;
				white-space: pre-wrap;
			}
		`,
	];

	@query('details')
	details!: HTMLDetailsElement;

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

	@property({ type: Boolean })
	open = false;

	@property()
	exception = '';

	willUpdate(changedProperties: Map<string | number | symbol, unknown>) {
		if (changedProperties.has('timestamp')) {
			this.date = new Date(this.timestamp);
		}
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('open')) {
			this.open ? this.details.setAttribute('open', 'true') : this.details.removeAttribute('open');
		}
	}

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
		});
	}

	private _searchMenuData: Array<{ label: string; href: () => string; icon: string; title: string }> = [
		{
			label: 'Search in Google',
			title: '@logViewer_searchThisMessageWithGoogle',
			href: () => `https://www.google.com/search?q=${this.renderedMessage}`,
			icon: 'https://www.google.com/favicon.ico',
		},
		{
			label: 'Search in Bing',
			title: 'Search this message with Bing',
			href: () => `https://www.bing.com/search?q=${this.renderedMessage}`,
			icon: 'https://www.bing.com/favicon.ico',
		},
		{
			label: 'Search in OurUmbraco',
			title: 'Search this message on Our Umbraco forums and docs',
			href: () => `https://our.umbraco.com/search?q=${this.renderedMessage}&content=wiki,forum,documentation`,
			icon: 'https://our.umbraco.com/assets/images/app-icons/favicon.png',
		},
		{
			label: 'Search in OurUmbraco with Google',
			title: 'Search Our Umbraco forums using Google',
			href: () =>
				`https://www.google.co.uk/?q=site:our.umbraco.com ${this.renderedMessage}&safe=off#q=site:our.umbraco.com ${
					this.renderedMessage
				} ${this.properties.find((property) => property.name === 'SourceContext')?.value}&safe=off"`,
			icon: 'https://www.google.com/favicon.ico',
		},
		{
			label: 'Search Umbraco Source',
			title: 'Search within Umbraco source code on Github',
			href: () =>
				`https://github.com/umbraco/Umbraco-CMS/search?q=${
					this.properties.find((property) => property.name === 'SourceContext')?.value
				}`,
			icon: 'https://github.githubassets.com/favicon.ico',
		},
		{
			label: 'Search Umbraco Issues',
			title: 'Search Umbraco Issues on Github',
			href: () =>
				`https://github.com/umbraco/Umbraco-CMS/issues?q=${
					this.properties.find((property) => property.name === 'SourceContext')?.value
				}`,
			icon: 'https://github.githubassets.com/favicon.ico',
		},
	];

	private _propertiesWithSearchMenu: Array<string> = ['HttpRequestNumber', 'SourceContext', 'MachineName'];

	private _findLogsWithProperty({ name, value }: LogMessagePropertyModel) {
		let queryString = '';

		if (isNaN(+(value ?? ''))) {
			queryString = name + "='" + value + "'";
		} else {
			queryString = name + '=' + value;
		}

		this.#logViewerContext?.setFilterExpression(queryString);
		this.#logViewerContext?.setCurrentPage(1);
		this.details.removeAttribute('open');
		this.#logViewerContext?.getLogs();
	}

	#setOpen(event: Event) {
		this.open = (event.target as HTMLDetailsElement).open;
	}

	render() {
		return html`
			<details @open=${this.#setOpen}>
				<summary>
					<div id="timestamp">${this.date?.toLocaleString()}</div>
					<div id="level">
						<umb-log-viewer-level-tag .level=${this.level ? this.level : 'Information'}></umb-log-viewer-level-tag>
					</div>
					<div id="machine">${this.properties.find((property) => property.name === 'MachineName')?.value}</div>
					<div id="message">${this.renderedMessage}</div>
				</summary>
				${this.exception ? html`<pre id="exception">${this.exception}</pre>` : ''}
				<ul id="properties-list">
					<li class="property">
						<div class="property-name">Timestamp</div>
						<div class="property-value">${this.date?.toLocaleString()}</div>
					</li>
					<li class="property">
						<div class="property-name">@MessageTemplate</div>
						<div class="property-value">${this.messageTemplate}</div>
					</li>
					${this.properties.map(
						(property) =>
							html`<li class="property">
								<div class="property-name">${property.name}:</div>
								<div class="property-value">
									${property.value}
									${this._propertiesWithSearchMenu.includes(property.name ?? '')
										? html`<uui-button
												compact
												@click=${() => {
													this._findLogsWithProperty(property);
												}}
												look="secondary"
												label="Find logs with ${property.name}"
												title="Find logs with ${property.name}"
												><uui-icon name="umb:search"></uui-icon
										  ></uui-button>`
										: ''}
								</div>
							</li>`
					)}
				</ul>
				<umb-button-with-dropdown look="secondary" placement="bottom-start" id="search-button" label="Search">
					<uui-icon name="umb:search"></uui-icon>Search
					<ul id="search-menu" slot="dropdown">
						${this._searchMenuData.map(
							(menuItem) => html`
								<li>
									<uui-menu-item
										class="search-item"
										href="${menuItem.href()}"
										target="_blank"
										label="${menuItem.label}"
										title="${menuItem.title}">
										<img slot="icon" src="${menuItem.icon}" width="16" height="16" alt="" />
									</uui-menu-item>
								</li>
							`
						)}
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
