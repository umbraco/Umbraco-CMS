import { UmbDocumentUrlRepository } from '../../../repository/url/document-url.repository.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../document-workspace.context-token.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import { css, customElement, html, map, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-document-workspace-view-info-links')
export class UmbDocumentWorkspaceViewInfoLinksElement extends UmbLitElement {
	#documentUrlRepository = new UmbDocumentUrlRepository(this);

	@state()
	private _isNew = false;

	@state()
	private _unique?: string;

	@state()
	private _variantOptions?: Array<UmbDocumentVariantOptionModel>;

	@state()
	private _lookup: Record<string, string> = {};

	constructor() {
		super();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			context.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () => {
				this.#requestUrls();
			});
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				observeMultiple([context.isNew, context.unique, context.variantOptions]),
				([isNew, unique, variantOptions]) => {
					this._isNew = isNew === true;
					this._unique = unique;
					this._variantOptions = variantOptions;
					this.#requestUrls();
				},
			);
		});
	}

	async #requestUrls() {
		if (this._isNew) return;

		if (!this._unique) throw new Error('Document unique is required');

		const { data } = await this.#documentUrlRepository.requestItems([this._unique]);

		if (data?.length) {
			data[0].urls.forEach((item) => {
				if (item.culture && item.url) {
					this._lookup[item.culture] = item.url;
				}
			});
			this.requestUpdate('_lookup');
		}
	}

	#getStateLocalizationKey(variantOption: UmbDocumentVariantOptionModel) {
		switch (variantOption.variant?.state) {
			case null:
			case undefined:
			case DocumentVariantStateModel.NOT_CREATED:
				return 'content_notCreated';
			case DocumentVariantStateModel.DRAFT:
				return 'content_itemNotPublished';
			case DocumentVariantStateModel.PUBLISHED:
				return 'content_routeErrorCannotRoute';
			default:
				return 'content_parentNotPublishedAnomaly';
		}
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('general_links')}>
				${when(
					this._isNew,
					() => this.#renderNotCreated(),
					() => this.#renderUrls(),
				)}
			</uui-box>
		`;
	}

	#renderNotCreated() {
		return html`
			<div class="link-item">
				<span>
					<em><umb-localize key="content_notCreated"></umb-localize></em>
				</span>
			</div>
		`;
	}

	#renderUrls() {
		if (!this._variantOptions?.length) return nothing;
		return map(this._variantOptions, (variantOption) => this.#renderUrl(variantOption));
	}

	#renderUrl(variantOption: UmbDocumentVariantOptionModel) {
		const varies = !!variantOption.culture;
		const culture = varies ? variantOption.culture! : variantOption.language.unique;
		const url = this._lookup[culture];
		return when(
			url,
			() => html`
				<a class="link-item" href=${url} target="_blank">
					<span>
						<span class="culture">${varies ? culture : nothing}</span>
						<span>${url}</span>
					</span>
					<uui-icon name="icon-out"></uui-icon>
				</a>
			`,
			() => html`
				<div class="link-item">
					<span>
						${when(varies, () => html`<span class="culture">${culture}</span>`)}
						<em><umb-localize key=${this.#getStateLocalizationKey(variantOption)}></umb-localize></em>
					</span>
				</div>
			`,
		);
	}

	static override styles = [
		css`
			uui-box {
				--uui-box-default-padding: 0;
			}

			.link-item {
				display: flex;
				justify-content: space-between;
				align-items: center;
				gap: var(--uui-size-6);

				padding: var(--uui-size-space-4) var(--uui-size-space-6);

				&:is(a) {
					cursor: pointer;
					color: inherit;
					text-decoration: none;
				}

				&:is(a):hover {
					background: var(--uui-color-divider);
				}

				& > span {
					display: flex;
					align-items: center;
					gap: var(--uui-size-6);
				}

				.culture {
					color: var(--uui-color-divider-emphasis);
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
