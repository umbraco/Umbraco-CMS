import type { LogViewerDateRange, UmbLogViewerWorkspaceContext } from '../workspace/logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../workspace/logviewer-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { query as getQuery, path, toQueryString } from '@umbraco-cms/backoffice/router';
import { UMB_THEME_CONTEXT } from '@umbraco-cms/backoffice/themes';

@customElement('umb-log-viewer-date-range-selector')
export class UmbLogViewerDateRangeSelectorElement extends UmbLitElement {
	@state()
	private _startDate = '';

	@state()
	private _endDate = '';
	
	@queryAll('input')
	private _inputs!: NodeListOf<HTMLInputElement>;

	@property({ type: Boolean, reflect: true })
	horizontal = false;

	@property({ type: String, reflect: true })
	theme = '';

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.addEventListener('input', this.#setDates);
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
		});

		this.consumeContext(UMB_THEME_CONTEXT, (instance) => {
			this.observe(instance.theme, (themeAlias) => {
				this.theme = themeAlias;
			});
		});
	}
	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.removeEventListener('input', this.#setDates);
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(
			this.#logViewerContext.dateRange,
			(dateRange: LogViewerDateRange) => {
				this._startDate = dateRange.startDate;
				this._endDate = dateRange.endDate;
			},
			'_observeDateRange',
		);
	}

	#setDates() {
		this._inputs.forEach((input) => {
			if (input.id === 'start-date') {
				this._startDate = input.value;
			} else if (input.id === 'end-date') {
				this._endDate = input.value;
			}
		});
		this.#logViewerContext?.setDateRange({ startDate: this._startDate, endDate: this._endDate });

		const query = getQuery();
		const qs = toQueryString({
			...query,
			startDate: this._startDate,
			endDate: this._endDate,
		});

		window.history.pushState({}, '', `${path()}?${qs}`);
	}

	override render() {
		return html`
			<div class="input-container">
				<uui-label for="start-date">From:</uui-label>
				<input 
					@click=${(e: Event) => {
						(e.target as HTMLInputElement).showPicker();
					}}
					id="start-date"
					type="date"
					label="From"
					.max=${this.#logViewerContext?.today ?? ''}
					.value=${this._startDate} />
			</div>
			<div class="input-container">
				<uui-label for="end-date">To: </uui-label>
				<input
					@click=${(e: Event) => {
						(e.target as HTMLInputElement).showPicker();
					}}
					id="end-date"
					type="date"
					label="To"
					.min=${this._startDate}
					.max=${this.#logViewerContext?.today ?? ''}
					.value=${this._endDate} />
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}

			:host([theme='umb-dark-theme']) input[type='date']::-webkit-calendar-picker-indicator {
				filter: invert(1);
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
				outline: none;
				position: relative;
				border-bottom: 2px solid transparent;
			}

			/* find out better validation for that  */
			input:out-of-range {
				border-color: var(--uui-color-danger);
			}

			:host([horizontal]) .input-container {
				display: flex;
				align-items: baseline;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-date-range-selector': UmbLogViewerDateRangeSelectorElement;
	}
}
