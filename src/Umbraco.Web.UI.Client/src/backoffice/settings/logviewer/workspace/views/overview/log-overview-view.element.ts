import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { clamp } from 'lodash-es';
import {
	LogViewerDateRange,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../../logviewer.context';
import { LogLevelCountsModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

//TODO: add a disabled attribute to the show more button when the total number of items is correctly returned from the endpoint
@customElement('umb-log-viewer-overview-view')
export class UmbLogViewerOverviewViewElement extends UmbLitElement {
	static styles = [
		css`
			:host {
				display: block;
			}

			#logviewer-layout {
				margin: 20px;
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

			#errors {
				grid-area: 2 / 1 / 3 / 2;
			}

			#level {
				grid-area: 2 / 2 / 3 / 3;
			}

			#log-lever {
				color: var(--uui-color-positive);
				text-align: center;
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
				--uui-box-default-padding: 0 var(--uui-size-space-5, 18px) var(--uui-size-space-5, 18px)
					var(--uui-size-space-5, 18px);
			}

			#common-messages-container > uui-box {
				height: 100%;
			}

			uui-label:nth-of-type(2) {
				display: block;
				margin-top: var(--uui-size-space-5);
			}

			button {
				all: unset;
				display: flex;
				align-items: center;
				cursor: pointer;
			}

			button:focus {
				outline: 1px solid var(--uui-color-focus);
			}

			button.active {
				text-decoration: line-through;
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

			#error-count {
				font-size: 4rem;
				text-align: center;
				color: var(--uui-color-danger);
			}
		`,
	];

	@state()
	private _totalLogCount = 0;

	@state()
	private _errorCount = 0;

	@state()
	private _logLevelCountFilter: string[] = [];

	@state()
	private logLevelCount: [string, number][] = [];

	@state()
	private _logLevelCount: LogLevelCountsModel | null = null;

	@state()
	private _startDate = '';

	@state()
	private _endDate = '';

	setLogLevelCount() {
		this.logLevelCount = this._logLevelCount
			? Object.entries(this._logLevelCount).filter(([level, number]) => !this._logLevelCountFilter.includes(level))
			: [];
		this._totalLogCount = this._logLevelCount
			? this.logLevelCount.flatMap((arr) => arr[1]).reduce((acc, count) => acc + count, 0)
			: 0;
	}

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;

		this.observe(this.#logViewerContext.logCount, (logLevel) => {
			this._logLevelCount = logLevel ?? null;
			this._errorCount = this._logLevelCount?.error ?? 0;
			this.setLogLevelCount();
		});

		this.observe(this.#logViewerContext.dateRange, (dateRange: LogViewerDateRange) => {
			this._startDate = dateRange?.startDate;
			this._endDate = dateRange?.endDate;
		});
	}

	protected willUpdate(_changedProperties: Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('_logLevelCountFilter')) {
			this.setLogLevelCount();
			this._totalLogCount = this._logLevelCount
				? this.logLevelCount.flatMap((arr) => arr[1]).reduce((acc, count) => acc + count, 0)
				: 0;
		}
	}

	#calculatePercentage(partialValue: number) {
		if (this._totalLogCount === 0) return 0;
		const percent = Math.round((100 * partialValue) / this._totalLogCount);
		return clamp(percent, 0, 99);
	}

	#setDates(event: Event) {
		const target = event.target as HTMLInputElement;
		if (target.id === 'start-date') {
			this._startDate = target.value;
		} else if (target.id === 'end-date') {
			this._endDate = target.value;
		}
		const newDateRange: LogViewerDateRange = { startDate: this._startDate, endDate: this._endDate };
		this.#logViewerContext?.setDateRange(newDateRange);
	}

	#setCountFilter(level: string) {
		if (this._logLevelCountFilter.includes(level)) {
			this._logLevelCountFilter = this._logLevelCountFilter.filter((item) => item !== level);
			return;
		}

		this._logLevelCountFilter = [...this._logLevelCountFilter, level];
	}

	setCurrentQuery(query: string) {
		this.#logViewerContext?.setFilterExpression(query);
	}

	render() {
		return html`
			<div id="logviewer-layout">
				<div id="info">
					<uui-box id="time-period" headline="Time Period">
						<umb-log-viewer-date-range-selector></umb-log-viewer-date-range-selector>
					</uui-box>

					<uui-box id="errors" headline="Number of Errors">
						<h1 id="error-count">${this._errorCount}</h1>
					</uui-box>

					<uui-box id="level" headline="Log level">
						<h1 id="log-lever">Info</h1>
					</uui-box>

					<uui-box id="types" headline="Log types">
						<div id="log-types-container">
							<div id="legend">
								<ul>
									${this._logLevelCount
										? Object.keys(this._logLevelCount).map(
												(level) =>
													html`<li>
														<button
															@click=${(e: Event) => {
																(e.target as HTMLElement)?.classList.toggle('active');
																this.#setCountFilter(level);
															}}>
															<uui-icon
																name="umb:record"
																style="color: var(--umb-log-viewer-${level.toLowerCase()}-color);"></uui-icon
															>${level}
														</button>
													</li>`
										  )
										: ''}
								</ul>
							</div>
							<umb-donut-chart>
								${this._logLevelCount
									? this.logLevelCount.map(
											([level, number]) =>
												html`<umb-donut-slice
													.name=${level}
													.amount=${number}
													.percent=${this.#calculatePercentage(number)}
													.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice> `
									  )
									: ''}
							</umb-donut-chart>
						</div>
					</uui-box>
				</div>

				<div id="saved-searches-container">
					<umb-log-viewer-saved-searches-overview></umb-log-viewer-saved-searches-overview>
				</div>

				<div id="common-messages-container">
					<umb-log-viewer-message-templates-overview></umb-log-viewer-message-templates-overview>
				</div>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-overview-view': UmbLogViewerOverviewViewElement;
	}
}
