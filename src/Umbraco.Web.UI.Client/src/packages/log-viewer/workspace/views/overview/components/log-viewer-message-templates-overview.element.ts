import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LogTemplateResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-message-templates-overview')
export class UmbLogViewerMessageTemplatesOverviewElement extends UmbLitElement {
	#itemsPerPage = 10;
	#currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _messageTemplates: Array<LogTemplateResponseModel> = [];

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#getMessageTemplates();
		this.#observeStuff();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	#observeStuff() {
		this.observe(this._logViewerContext?.messageTemplates, (templates) => {
			this._messageTemplates = templates?.items ?? [];
			this._total = templates?.total ?? 0;
		});
	}

	#getMessageTemplates() {
		const skip = this.#currentPage * this.#itemsPerPage - this.#itemsPerPage;
		this._logViewerContext?.getMessageTemplates(skip, this.#itemsPerPage);
	}

	#onChangePage(event: UUIPaginationEvent) {
		this.#currentPage = event.target.current;
		this.#getMessageTemplates();
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('logViewer_commonLogMessages')} id="saved-searches">
				<p style="font-style: italic;">${this.localize.term('logViewer_totalUniqueMessageTypes', this._total)}</p>

				<uui-table>
					${this._messageTemplates
						? this._messageTemplates.map(
								(template) =>
									html`<uui-table-row>
										<uui-table-cell>
											<a
												href=${`section/settings/workspace/logviewer/view/search/?lq=${encodeURIComponent(
													`@MessageTemplate='${template.messageTemplate}'`,
												)}`}>
												<span>${template.messageTemplate}</span> <span>${template.count}</span>
											</a>
										</uui-table-cell>
									</uui-table-row>`,
							)
						: ''}
				</uui-table>
				${this._total > this.#itemsPerPage
					? html`<uui-pagination
							.current=${this.#currentPage}
							.total=${Math.ceil(this._total / this.#itemsPerPage)}
							firstlabel=${this.localize.term('general_first')}
							previouslabel=${this.localize.term('general_previous')}
							nextlabel=${this.localize.term('general_next')}
							lastlabel=${this.localize.term('general_last')}
							@change=${this.#onChangePage}></uui-pagination>`
					: nothing}
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-pagination {
				margin-top: var(--uui-size-layout-1);
			}

			#show-more-templates-btn {
				margin-top: var(--uui-size-space-5);
			}

			a {
				display: flex;
				align-items: center;
				justify-content: space-between;
				text-decoration: none;
				color: inherit;
			}

			uui-table-cell {
				padding: 10px 20px;
				height: unset;
			}

			uui-table-row {
				cursor: pointer;
			}

			uui-table-row:hover > uui-table-cell {
				background-color: var(--uui-color-surface-alt);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-message-templates-overview': UmbLogViewerMessageTemplatesOverviewElement;
	}
}
