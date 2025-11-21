import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LogLevelCountsReponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-log-types-chart')
export class UmbLogViewerLogTypesChartElement extends UmbLitElement {
	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#logViewerContext?.getLogCount();
		this.#observeStuff();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	@state()
	private _dateRange = { startDate: '', endDate: '' };

	@state()
	private _logLevelCountResponse: LogLevelCountsReponseModel | null = null;

	@state()
	private _logLevelCount: [string, number][] = [];

	@state()
	private _logLevelCountFilter: string[] = [];

	@state()
	private _logLevelKeys: string[] = [];

	protected override willUpdate(_changedProperties: Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('_logLevelCountFilter') || _changedProperties.has('_logLevelCountResponse')) {
			this.setLogLevelCount();
		}
	}

	#setCountFilter(level: string) {
		if (this._logLevelCountFilter.includes(level)) {
			this._logLevelCountFilter = this._logLevelCountFilter.filter((item) => item !== level);
			return;
		}

		this._logLevelCountFilter = [...this._logLevelCountFilter, level];
	}

	setLogLevelCount() {
		if (this._logLevelCountResponse) {
			this._logLevelKeys = Object.keys(this._logLevelCountResponse);
			this._logLevelCount = Object.entries(this._logLevelCountResponse).filter(
				([level]) => !this._logLevelCountFilter.includes(level),
			);
		} else {
			this._logLevelKeys = [];
			this._logLevelCount = [];
		}
	}

	#observeStuff() {
		this.observe(this._logViewerContext?.logCount, (logLevel) => {
			this._logLevelCountResponse = logLevel ?? null;
			this.setLogLevelCount();
		});

		this.observe(this._logViewerContext?.dateRange, (dateRange) => {
			if (dateRange) {
				this._dateRange = dateRange;
			}
		});
	}

	#buildSearchUrl(level: string): string {
		const params = new URLSearchParams();
		params.set('loglevels', level);
		if (this._dateRange.startDate) {
			params.set('startDate', this._dateRange.startDate);
		}
		if (this._dateRange.endDate) {
			params.set('endDate', this._dateRange.endDate);
		}
		return `section/settings/workspace/logviewer/view/search/?${params.toString()}`;
	}

	override render() {
		return html`
			<uui-box id="types" headline="Log types">
				<div id="log-types-container">
					<div id="legend">
						<ul>
							${repeat(
								this._logLevelKeys,
								(level) => level,
								(level) =>
									html`<li>
										<button
											@click=${(e: Event) => {
												(e.target as HTMLElement)?.classList.toggle('active');
												this.#setCountFilter(level);
											}}>
											<uui-icon
												name="icon-record"
												style="color: var(--umb-log-viewer-${level.toLowerCase()}-color);"></uui-icon
											>${level}
										</button>
									</li>`,
							)}
						</ul>
					</div>
					<umb-donut-chart
						.description=${'In chosen date range you have this number of log message of type:'}
						show-inline-numbers
						show-description>
						${repeat(
							this._logLevelCount,
							([level]) => level,
							([level, number]) =>
								html`<umb-donut-slice
									.name=${level}
									.amount=${number}
									.kind=${'messages'}
									.href=${this.#buildSearchUrl(level)}
									.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice>`,
						)}
					</umb-donut-chart>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			#log-types-container {
				display: flex;
				gap: var(--uui-size-space-4);
				flex-direction: column-reverse;
				align-items: center;
				justify-content: space-between;
			}

			@media (min-width: 768px) {
				#log-types-container {
					flex-direction: row;
					align-items: flex-start;
				}
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
				background:
					radial-gradient(white 40%, transparent 41%),
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-types-chart': UmbLogViewerLogTypesChartElement;
	}
}
