import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { LogLevelModel, LogMessagePropertyPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { query as getQuery, toQueryString } from '@umbraco-cms/backoffice/router';

//TODO: check how to display EventId field in the message properties
@customElement('umb-log-viewer-message')
export class UmbLogViewerMessageElement extends UmbLitElement {
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
	properties: Array<LogMessagePropertyPresentationModel> = [];

	@property({ type: Boolean })
	open = false;

	@property()
	exception = '';

	override willUpdate(changedProperties: Map<string | number | symbol, unknown>) {
		if (changedProperties.has('timestamp')) {
			this.date = new Date(this.timestamp);
		}
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('open')) {
			if (this.open) {
				this.details.setAttribute('open', 'true');
			} else {
				this.details.removeAttribute('open');
			}
		}
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

	private _findLogsWithProperty({ name, value }: LogMessagePropertyPresentationModel) {
		if (!name) return '';

		let query = getQuery();
		let sanitizedValue = value ?? '';

		if (isNaN(+(value ?? ''))) {
			sanitizedValue = "'" + value + "'";
		}

		query = {
			...query,
			lq: encodeURIComponent(`${name}=${sanitizedValue}`),
		};

		const queryString = toQueryString(query);

		return queryString;
	}

	#setOpen(event: Event) {
		this.open = (event.target as HTMLDetailsElement).open;
	}

	override render() {
		return html`
			<details @open=${this.#setOpen}>
				<summary>
					<div id="timestamp">${this.date?.toLocaleString()}</div>
					<div id="level">
						<umb-log-viewer-level-tag .level=${this.level ? this.level : 'Information'}></umb-log-viewer-level-tag>
					</div>
					<div id="machine">${this.properties.find((property) => property.name === 'MachineName')?.value}</div>
					<div id="message"><uui-scroll-container>${this.renderedMessage}</uui-scroll-container></div>
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
												look="secondary"
												label="Find logs with ${property.name}"
												title="Find logs with ${property.name}"
												href=${`section/settings/workspace/logviewer/view/search/?${this._findLogsWithProperty(
													property,
												)}`}>
												<uui-icon name="icon-search"></uui-icon>
											</uui-button>`
										: ''}
								</div>
							</li>`,
					)}
				</ul>
				<umb-dropdown look="secondary" placement="bottom-start" id="search-button" label="Search">
					<span slot="label"><uui-icon name="icon-search"></uui-icon> Search</span>
					${this._searchMenuData.map(
						(menuItem) => html`
							<uui-menu-item
								class="search-item"
								href="${menuItem.href()}"
								target="_blank"
								label="${menuItem.label}"
								title="${menuItem.title}">
								<img slot="icon" src="${menuItem.icon}" width="16" height="16" alt="" />
							</uui-menu-item>
						`,
					)}
				</umb-dropdown>
			</details>
		`;
	}

	static override styles = [
		UmbTextStyles,
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

			summary:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}

			#properties-list {
				padding: 0;
				list-style: none;
			}

			.property {
				padding: 10px 20px;
				display: flex;
				border-top: 1px solid var(--uui-color-border);
			}

			.property:first-child {
				border-top: transparent;
			}

			#properties-list,
			pre {
				margin: 0 var(--uui-size-layout-1);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-background);
				margin-bottom: var(--uui-size-space-3);
			}

			pre {
				border-left: 4px solid #d42054;
				display: block;
				font-family:
					Lato,
					Helvetica Neue,
					Helvetica,
					Arial,
					sans-serif;
				line-height: 20px;
				overflow-x: auto;
				padding: 9.5px;
				white-space: pre-wrap;
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

			uui-menu-item {
				--uui-menu-item-flat-structure: 1;
			}

			#message {
				flex: 6 0 14ch;
				word-break: break-all;
			}
			#search-button {
				margin-left: var(--uui-size-layout-1);
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

			.search-item {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-message': UmbLogViewerMessageElement;
	}
}
