import type { UmbLogLevelCounts } from '../../../../../log-viewer/types.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { umbFormatDonutShare } from '../../../../components/donut-chart/index.js';
import { classMap, css, html, customElement, nothing, state, repeat } from '@umbraco-cms/backoffice/external/lit';
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
		const shownTotal = this._logLevelCount.reduce((sum, [, count]) => sum + count, 0);
		return html`
			<uui-box id="types" headline=${this.localize.term('logViewer_logTypes')}>
				<p id="description">
					<umb-localize key="logViewer_logTypesChartDescription">
						In the chosen date range, you have this number of log messages grouped by type:
					</umb-localize>
				</p>
				<div id="log-types-container">
					<umb-donut-chart border-size="8" .description=${this.localize.term('logViewer_logTypes')}>
						${repeat(
							this._logLevelCount,
							([level]) => level,
							([level, number]) =>
								html`<umb-donut-slice
									.name=${level}
									.amount=${number}
									.kind=${this.localize.term('logViewer_messagesCount')}
									.href=${this.#buildSearchUrl(level)}
									.label=${this.localize.term('logViewer_viewLogsLabel', level)}
									.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice>`,
						)}
					</umb-donut-chart>
					<div id="legend">
						<ul>
							${repeat(
								this._logLevelKeys,
								([level]) => level,
								([level, count]) => {
									const shown = !this._logLevelCountFilter.includes(level);
									const viewLogsLabel = this.localize.term('logViewer_viewLogsLabel', level);
									return html`<li>
										<button
											class=${classMap({ active: !shown })}
											aria-pressed=${shown}
											@click=${() => this.#setCountFilter(level)}>
											<span
												class="swatch"
												aria-hidden="true"
												style="background: var(--umb-log-viewer-${level.toLowerCase()}-color);"></span>
											<span>${level}</span>
											<span class="count">
												${this.localize.number(count, {
													notation: 'compact',
													minimumFractionDigits: count > 1000 ? 1 : 0,
													maximumFractionDigits: 2,
												})}
											</span>
											<span class="share">
												${shown ? umbFormatDonutShare(count / shownTotal, this.localize) : nothing}
											</span>
										</button>
										<a
											class="view-logs"
											href=${this.#buildSearchUrl(level)}
											aria-label=${viewLogsLabel}
											title=${viewLogsLabel}>
											<uui-icon name="icon-search"></uui-icon>
										</a>
									</li>`;
								},
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

			#legend {
				width: 100%;
				display: flex;
				justify-content: center;
			}

			@container (min-width: 312px) {
				#log-types-container {
					display: flex;
					align-items: flex-start;
					column-gap: var(--uui-size-space-2);
				}

				umb-donut-chart {
					flex: 0 1 auto;
					min-width: 0;
				}

				#legend {
					flex: none;
					width: auto;
				}
			}

			button {
				all: unset;
				display: grid;
				grid-column: 1 / 5;
				grid-template-columns: subgrid;
				align-items: center;
				min-height: 24px;
				cursor: pointer;
			}

			.view-logs {
				display: inline-flex;
				align-items: center;
				justify-content: center;
				min-width: 24px;
				min-height: 24px;
				color: var(--uui-color-interactive);
			}

			.view-logs:hover {
				color: var(--uui-color-interactive-emphasis);
			}

			button:focus-visible,
			.view-logs:focus-visible {
				outline: 2px solid var(--uui-color-text);
				outline-offset: 2px;
				box-shadow: 0 0 0 2px var(--uui-color-surface);
			}

			button > * + * {
				padding-inline-start: var(--uui-size-space-1);
			}

			.swatch {
				width: var(--uui-size-3);
				height: var(--uui-size-3);
				border-radius: 50%;
				forced-color-adjust: none;
			}

			button.active {
				text-decoration: line-through;
			}

			ul {
				list-style: none;
				margin: 0;
				padding: 0;
				border: 0;
				font: inherit;
				display: grid;
				grid-template-columns: repeat(5, auto);
			}

			li {
				display: grid;
				grid-column: 1 / -1;
				grid-template-columns: subgrid;
			}

			.count,
			.share {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
				font-variant-numeric: tabular-nums;
				text-align: end;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-types-chart': UmbLogViewerLogTypesChartElement;
	}
}
