import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, queryAll, state } from 'lit/decorators.js';
import { query } from 'router-slot';
import {
	LogViewerDateRange,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-log-viewer-date-range-selector')
export class UmbLogViewerDateRangeSelectorElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
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
				height: 100%;
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
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getMessageTemplates(0, 10);
			this.#observeStuff();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.dateRange, (dateRange: LogViewerDateRange) => {
			this._startDate = dateRange?.startDate;
			this._endDate = dateRange?.endDate;
		});
	}

	#setDates() {
		this._inputs.forEach((input) => {
			if (input.id === 'start-date') {
				this._startDate = input.value;
			} else if (input.id === 'end-date') {
				this._endDate = input.value;
			}
		});
		const newDateRange: LogViewerDateRange = { startDate: this._startDate, endDate: this._endDate };
		this.#logViewerContext?.setDateRange(newDateRange);
	}

	render() {
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
                .value=${this._startDate}>
            </input>
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
                .value=${this._endDate}>
            </input>
        </div>
        `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-date-range-selector': UmbLogViewerDateRangeSelectorElement;
	}
}
