import type { UmbLogLevelCounts } from '../../../../../log-viewer/types.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
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
	private _logLevelCounts: UmbLogLevelCounts | null = null;

	@state()
	private _logLevelCount: [string, number][] = [];

	@state()
	private _logLevelCountFilter: string[] = [];

	@state()
	private _logLevelKeys: [string, number][] = [];

	protected override willUpdate(_changedProperties: Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('_logLevelCountFilter') || _changedProperties.has('_logLevelCounts')) {
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
		if (this._logLevelCounts) {
			const nonZeroEntries = Object.entries(this._logLevelCounts).filter(([, count]) => count > 0);
			this._logLevelKeys = nonZeroEntries;
			this._logLevelCount = nonZeroEntries.filter(([level]) => !this._logLevelCountFilter.includes(level));
		} else {
			this._logLevelKeys = [];
			this._logLevelCount = [];
		}
	}

	#observeStuff() {
		this.observe(this._logViewerContext?.logCount, (logLevel) => {
			this._logLevelCounts = logLevel ?? null;
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
			<uui-box id="types" headline=${this.localize.term('logViewer_logTypes')}>
				<p id="description">
					<umb-localize key="logViewer_logTypesChartDescription">
						In the chosen date range, you have this number of log messages grouped by type:
					</umb-localize>
				</p>
				<div id="log-types-container">
					<umb-donut-chart>
						${repeat(
							this._logLevelCount,
							([level]) => level,
							([level, number]) =>
								html`<umb-donut-slice
									.name=${level}
									.amount=${number}
									.kind=${this.localize.term('logViewer_messagesCount')}
									.href=${this.#buildSearchUrl(level)}
									.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice>`,
						)}
					</umb-donut-chart>
					<div id="legend">
						<ul>
							${repeat(
								this._logLevelKeys,
								([level]) => level,
								([level, count]) =>
									html`<li>
										<button
											@click=${(e: Event) => {
												(e.target as HTMLElement)?.classList.toggle('active');
												this.#setCountFilter(level);
											}}>
											<uui-icon
												name="icon-record"
												style="color: var(--umb-log-viewer-${level.toLowerCase()}-color);"></uui-icon>
											${level}
											<span class="count">
												(${this.localize.number(count, {
													notation: 'compact',
													minimumFractionDigits: count > 1000 ? 1 : 0,
													maximumFractionDigits: 2,
												})})
											</span>
										</button>
									</li>`,
							)}
						</ul>
					</div>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			uui-box {
				container-type: inline-size;
			}

			#description {
				text-align: center;
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				margin: 0 0 var(--uui-size-space-4) 0;
			}

			#log-types-container {
				display: grid;
				gap: var(--uui-size-space-4);
				grid-template-columns: 1fr;
				place-items: center;
			}

			umb-donut-chart {
				width: 100%;
				max-width: 200px;
			}

			#legend {
				width: 100%;
				display: flex;
				justify-content: center;
			}

			@container (min-width: 312px) {
				#log-types-container {
					grid-template-columns: auto 1fr;
					place-items: start;
				}

				umb-donut-chart {
					max-width: 200px;
				}

				#legend {
					width: auto;
					justify-content: flex-start;
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

			.count {
				margin-left: 0.3em;
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-types-chart': UmbLogViewerLogTypesChartElement;
	}
}
