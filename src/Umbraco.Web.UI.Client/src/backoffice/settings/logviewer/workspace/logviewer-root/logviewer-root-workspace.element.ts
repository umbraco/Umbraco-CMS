import '../donut-chart';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext } from './logviewer-root.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { SavedLogSearchModel } from '@umbraco-cms/backend-api';

const logLevels = {
	information: 171,
	debug: 39,
	warning: 31,
	error: 1,
	fatal: 0,
};

type LogLevel = keyof typeof logLevels;

//TODO make uui-input accept min and max values
@customElement('umb-logviewer-root-workspace')
export class UmbLogViewerRootWorkspaceElement extends UmbLitElement {
	static styles = [
		css`
			:host {
				display: block;

				--umb-log-viewer-debug-color: var(--uui-color-default-emphasis);
				--umb-log-viewer-information-color: var(--uui-color-positive-standalone);
				--umb-log-viewer-warning-color: var(--uui-color-warning-standalone);
				--umb-log-viewer-error-color: var(--uui-color-danger-standalone);
				--umb-log-viewer-fatal-color: var(--uui-color-default-standalone);
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}

			#logviewer-layout {
				margin: 20px;
			}

			#logviewer-layout {
				height: calc(100vh - 160px);
				display: grid;
				grid-template-columns: 7fr 2fr;
				grid-template-rows: 1fr 1fr;
				gap: 20px 20px;
				grid-auto-flow: row;
				grid-template-areas:
					'saved-searches info'
					'common-messages info';
			}

			#info {
				grid-area: info;
				align-self: start;
				display: grid;
				grid-template-columns: repeat(2, 1fr);
				grid-template-rows: repeat(4, 1fr);
				gap: 20px 20px;
			}

			#time-period {
				grid-area: 1 / 1 / 2 / 3;
			}

			#date-input-container {
			}

			#errors {
				grid-area: 2 / 1 / 3 / 2;
			}

			#level {
				grid-area: 2 / 2 / 3 / 3;
			}

			#types {
				grid-area: 3 / 1 / 5 / 3;
			}

			#log-types-container {
				display: flex;
				gap: var(--uui-size-space-4);
				flex-direction: column-reverse;
				align-items: center;
				justify-content: space-between;
			}

			#saved-searches-container {
				grid-area: saved-searches;
			}

			#common-messages-container {
				grid-area: common-messages;
			}

			#saved-searches-container > uui-box,
			#common-messages-container > uui-box {
				height: 100%;
			}

			input {
				font-family: inherit;
				padding: var(--uui-size-1) var(--uui-size-space-3);
				font-size: inherit;
				color: inherit;
				border-radius: 0;
				box-sizing: border-box;
				border: none;
				background: none;
				width: 100%;
				text-align: inherit;
				outline: none;
			}

			ul {
				list-style: none;
				margin: 0;
				padding: 0;
				border: 0;
				font-size: 100%;
				font: inherit;
				vertical-align: baseline;
			}

			li {
				display: flex;
				align-items: center;
			}

			li uui-icon {
				margin-right: 1em;
			}

			#chart {
				width: 150px;
				aspect-ratio: 1;
				background: radial-gradient(white 40%, transparent 41%),
					conic-gradient(
						var(--umb-log-viewer-debug-color) 0% 20%,
						var(--umb-log-viewer-information-color) 20% 40%,
						var(--umb-log-viewer-warning-color) 40% 60%,
						var(--umb-log-viewer-error-color) 60% 80%,
						var(--umb-log-viewer-fatal-color) 80% 100%
					);
				margin: 10px;
				display: inline-block;
				border-radius: 50%;
			}
		`,
	];

	get today() {
		const today = new Date();
		const dd = String(today.getDate()).padStart(2, '0');
		const mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
		const yyyy = today.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	get yesterday() {
		const today = new Date();
		const dd = String(today.getDate() - 1).padStart(2, '0');
		const mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
		const yyyy = today.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	@state()
	private _savedSearches: SavedLogSearchModel[] = [];

	@state()
	private _totalLogCount = 0;

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
		console.log('Loading something from somewhere');
	}

	create(): void {
		// Not relevant for this workspace
	}

	#logViewerContext = new UmbLogViewerWorkspaceContext(this);

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
		});
		await this.#logViewerContext.getSavedSearches();

		this._totalLogCount = Object.values(logLevels).reduce((acc, count) => acc + count, 0);
	}

	logPercentage(partialValue: number) {
		if (this._totalLogCount === 0) return 0;

		return Math.round((100 * partialValue) / this._totalLogCount);
	}

	#renderSearchItem(searchListItem: SavedLogSearchModel) {
		return html` <li>
			<uui-button
				label="${searchListItem.name}"
				title="${searchListItem.name}"
				href=${'settings/logViewer/search?lq=' + searchListItem.query}
				><uui-icon name="umb:search"></uui-icon>${searchListItem.name}</uui-button
			>
		</li>`;
	}

	render() {
		return html` 
		<umb-body-layout headline="Log Overview for today">
			<div id="logviewer-layout">
				<div id="info">

					<uui-box id="time-period" headline="Time Period">
						<div id="date-input-container">
							<uui-label for="start-date">From:</uui-label> 
							<input 
								id="start-date" 
								type="date" 
								label="From" 
								max="${this.today}" 
								.value=${this.yesterday}>
							</input>
							<uui-label for="end-date">To: </uui-label>
							<input 
								id="end-date" 
								type="date" 
								label="To" 
								max="${this.today}" 
								.value=${this.today}>
							</input>
						</div>
					</uui-box>

					<uui-box id="errors" headline="Number of Errors"></uui-box>

					<uui-box id="level" headline="Log level"></uui-box>

					<uui-box id="types" headline="Log types">
						<div id="log-types-container">
							<div id="legend">
								<ul>
									${Object.keys(logLevels).map(
										(level) =>
											html`<li>
												<uui-icon name="umb:record" style="color: var(--umb-log-viewer-${level}-color);"></uui-icon
												>${level}
											</li>`
									)}
								</ul>
							</div>
							<umb-donut-chart>
							${Object.entries(logLevels).map(
								([level, number]) =>
									html`<umb-donut-slice
										.percent=${this.logPercentage(number)}
										.color=${`var(--umb-log-viewer-${level}-color)`}></umb-donut-slice> `
							)}
							</umb-donut-chart>
						</div>
					</uui-box>
				</div>

				<div id="saved-searches-container">
					<uui-box id="saved-searches" headline="Saved searches">
						<ul>${this._savedSearches.map(this.#renderSearchItem)}</ul>
					</uui-box>
				</div>

				<div id="common-messages-container">
					<uui-box headline="Common Log Messages" id="saved-searches">
						${this._savedSearches.map((search) => html`<div>${search.name}</div>`)}
					</uui-box>
				</div>
			</div>
		</umb-body-layout>`;
	}
}

export default UmbLogViewerRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-root-workspace': UmbLogViewerRootWorkspaceElement;
	}
}
