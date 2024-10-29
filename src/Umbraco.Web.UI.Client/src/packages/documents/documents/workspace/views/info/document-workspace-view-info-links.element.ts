import { UmbDocumentUrlRepository } from '../../../repository/url/document-url.repository.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../document-workspace.context-token.js';
import type { UmbDocumentUrlModel } from '../../../repository/url/types.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

@customElement('umb-document-workspace-view-info-links')
export class UmbDocumentWorkspaceViewInfoLinksElement extends UmbLitElement {
	#documentUrlRepository = new UmbDocumentUrlRepository(this);

	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _invariantCulture = 'en-US';

	@state()
	private _items?: Array<UmbDocumentUrlModel>;

	constructor() {
		super();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			context.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () => {
				this.#requestUrls();
			});
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#requestUrls();
		});
	}

	async #requestUrls() {
		const unique = this.#workspaceContext?.getUnique();
		if (!unique) throw new Error('Document unique is required');

		const { data } = await this.#documentUrlRepository.requestItems([unique]);

		if (data?.length) {
			this._items = data[0].urls;
		}
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('general_links')} style="--uui-box-default-padding: 0;">
				<div id="link-section">
					${when(
						this._items?.length,
						() => this.#renderUrls(),
						() => this.#renderUnpublished(),
					)}
				</div>
			</uui-box>
		`;
	}

	#renderUrls() {
		if (!this._items) return nothing;
		return html`
			${repeat(
				this._items!,
				(item) => item.culture,
				(item) => html`
					<a href=${item.url ?? '#'} target="_blank" class="link-item">
						<span class="culture">${item.culture}</span>
						<span class="url">${item.url}</span>
						<uui-icon name="icon-out"></uui-icon>
					</a>
				`,
			)}
		`;
	}

	#renderUnpublished() {
		return html`
			<div class="link-item">
				<span class="culture">${this._invariantCulture}</span>
				<span class="url">
					<em><umb-localize key="content_parentNotPublishedAnomaly"></umb-localize></em>
				</span>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#link-section {
				display: flex;
				flex-direction: column;
				text-align: left;
			}

			.link-item {
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				display: grid;
				grid-template-columns: auto 1fr auto;
				gap: var(--uui-size-6);
				color: inherit;
				text-decoration: none;

				&:is(a) {
					cursor: pointer;
				}

				&:is(a):hover {
					background: var(--uui-color-divider);
				}

				.culture {
					color: var(--uui-color-divider-emphasis);
				}

				uui-icon {
					margin-right: var(--uui-size-space-2);
					vertical-align: middle;
				}
			}
		`,
	];
}

export default UmbDocumentWorkspaceViewInfoLinksElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-info-links': UmbDocumentWorkspaceViewInfoLinksElement;
	}
}
