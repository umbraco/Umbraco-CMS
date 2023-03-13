import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { LogLevelCountsModel } from '@umbraco-cms/backend-api';

@customElement('umb-log-viewer-log-types-chart')
export class UmbLogViewerLogTypesChartElement extends UmbLitElement {
	static styles = [
		css`
			#log-types-container {
				display: flex;
				gap: var(--uui-size-space-4);
				flex-direction: column-reverse;
				align-items: center;
				justify-content: space-between;
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

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getLogCount();
			this.#observeStuff();
		});
	}

	@state()
	private _logLevelCount: LogLevelCountsModel | null = null;

	@state()
	private logLevelCount: [string, number][] = [];

	@state()
	private _logLevelCountFilter: string[] = [];

	protected willUpdate(_changedProperties: Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('_logLevelCountFilter')) {
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
		this.logLevelCount = this._logLevelCount
			? Object.entries(this._logLevelCount).filter(([level, number]) => !this._logLevelCountFilter.includes(level))
			: [];
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.logCount, (logLevel) => {
			this._logLevelCount = logLevel ?? null;
			this.setLogLevelCount();
		});
	}

	render() {
		return html`
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
					<umb-donut-chart .description=${'In chosen date range you have this number of log message of type:'}>
						${this._logLevelCount
							? this.logLevelCount.map(
									([level, number]) =>
										html`<umb-donut-slice
											.name=${level}
											.amount=${number}
											.kind=${'messages'}
											.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice> `
							  )
							: ''}
					</umb-donut-chart>
				</div>
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-types-chart': UmbLogViewerLogTypesChartElement;
	}
}
