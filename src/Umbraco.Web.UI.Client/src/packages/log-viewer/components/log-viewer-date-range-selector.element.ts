import type { LogViewerDateRange, UmbLogViewerWorkspaceContext } from '../workspace/logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../workspace/logviewer-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { query as getQuery, path, toQueryString } from '@umbraco-cms/backoffice/router';

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

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.addEventListener('input', this.#setDates);
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
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
				<umb-input-date
					@click=${(e: Event) => {
						(e.target as HTMLInputElement).showPicker();
					}}
					id="start-date"
					type="date"
					label="From"
					.max=${this.#logViewerContext?.today ?? ''}
					.value=${this._startDate}></umb-input-date>
			</div>
			<div class="input-container">
				<uui-label for="end-date">To: </uui-label>
				<umb-input-date
					@click=${(e: Event) => {
						(e.target as HTMLInputElement).showPicker();
					}}
					id="end-date"
					type="date"
					label="To"
					.min=${this._startDate}
					.max=${this.#logViewerContext?.today ?? ''}
					.value=${this._endDate}></umb-input-date>
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
			umb-input-date {
				width: 100%;
			}

			:host([horizontal]) .input-container {
				display: flex;
				align-items: baseline;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-date-range-selector': UmbLogViewerDateRangeSelectorElement;
	}
}
