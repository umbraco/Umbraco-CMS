import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { PagedLogTemplateModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';

//TODO: fix pagination bug when API is fixed
@customElement('umb-log-viewer-message-templates-overview')
export class UmbLogViewerMessageTemplatesOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
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

	@state()
	private _messageTemplates: PagedLogTemplateModel | null = null;

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getMessageTemplates(0, 10);
			this.#observeStuff();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.messageTemplates, (templates) => {
			this._messageTemplates = templates ?? null;
		});
	}

	#getMessageTemplates() {
		const take = this._messageTemplates?.items?.length ?? 0;
		this.#logViewerContext?.getMessageTemplates(0, take + 10);
	}

	#renderSearchItem = (searchListItem: SavedLogSearchModel) => {
		return html` <li>
			<uui-button
				@click=${() => {
					this.#setCurrentQuery(searchListItem.query ?? '');
				}}
				label="${searchListItem.name ?? ''}"
				title="${searchListItem.name ?? ''}"
				href=${'/section/settings/logviewer/search?lq=' + searchListItem.query}
				><uui-icon name="umb:search"></uui-icon>${searchListItem.name}</uui-button
			>
		</li>`;
	};

	#setCurrentQuery = (query: string) => {
		this.#logViewerContext?.setFilterExpression(query);
	};

	render() {
		return html`
			<uui-box headline="Common Log Messages" id="saved-searches">
				<p style="font-style: italic;">Total Unique Message types: ${this._messageTemplates?.total}</p>

				<uui-table>
					${this._messageTemplates
						? this._messageTemplates.items.map(
								(template) =>
									html`<uui-table-row
										><uui-table-cell>
											<a
												@click=${() => {
													this.#setCurrentQuery(`@MessageTemplate='${template.messageTemplate}'` ?? '');
												}}
												href=${'/section/settings/logviewer/search?lg=@MessageTemplate%3D' + template.messageTemplate}>
												<span>${template.messageTemplate}</span> <span>${template.count}</span>
											</a>
										</uui-table-cell>
									</uui-table-row>`
						  )
						: ''}
				</uui-table>

				<uui-button
					id="show-more-templates-btn"
					look="primary"
					@click=${this.#getMessageTemplates}
					label="Show more templates"
					>Show more</uui-button
				>
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-message-templates-overview': UmbLogViewerMessageTemplatesOverviewElement;
	}
}
