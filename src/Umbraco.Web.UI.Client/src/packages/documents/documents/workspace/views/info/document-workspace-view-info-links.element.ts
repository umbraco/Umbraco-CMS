import { UmbDocumentUrlRepository } from '../../../repository/url/document-url.repository.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../document-workspace.context-token.js';
import type { UmbDocumentVariantOptionModel } from '../../../types.js';
import type { UmbDocumentUrlModel } from '../../../repository/url/types.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { debounce } from '@umbraco-cms/backoffice/utils';

interface UmbDocumentInfoViewLink {
	culture: string;
	url: string | undefined;
	state: DocumentVariantStateModel | null | undefined;
}

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
	private _loading = false;

	@state()
	private _links: Array<UmbDocumentInfoViewLink> = [];

	#urls: Array<UmbDocumentUrlModel> = [];

	#documentWorkspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#eventContext = context;

			this.#eventContext.removeEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadRequest as unknown as EventListener,
			);

			this.#eventContext.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadRequest as unknown as EventListener,
			);
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#documentWorkspaceContext = context;
			this.observe(observeMultiple([context.isNew, context.unique]), ([isNew, unique]) => {
				if (!unique) return;
				this._isNew = isNew === true;

				if (unique !== this._unique) {
					this._unique = unique;
					this.#requestUrls();
				}
			});

			this.observe(context.variantOptions, (variantOptions) => {
				this._variantOptions = variantOptions;
				this.#setLinks();
			});
		});
	}

	#setLinks() {
		const possibleVariantCultures = this._variantOptions?.map((variantOption) => variantOption.culture) ?? [];
		const possibleUrlCultures = this.#urls.map((link) => link.culture);
		const possibleCultures = [...new Set([...possibleVariantCultures, ...possibleUrlCultures])].filter(
			Boolean,
		) as string[];

		const links: Array<UmbDocumentInfoViewLink> = possibleCultures.map((culture) => {
			const url = this.#urls.find((link) => link.culture === culture)?.url;
			const state = this._variantOptions?.find((variantOption) => variantOption.culture === culture)?.variant?.state;
			return { culture, url, state };
		});

		this._links = links;
	}

	async #requestUrls() {
		if (this._isNew) return;
		if (!this._unique) return;

		this._loading = true;
		this.#urls = [];

		const { data } = await this.#documentUrlRepository.requestItems([this._unique]);

		if (data?.length) {
			const item = data[0];
			this.#urls = item.urls;
			this.#setLinks();
		}

		this._loading = false;
	}

	#getStateLocalizationKey(state: DocumentVariantStateModel | null | undefined): string {
		switch (state) {
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

	#debounceRequestUrls = debounce(() => this.#requestUrls(), 50);

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		// TODO: Introduce "Published Event". We only need to update the url when the document is published.
		if (event.getUnique() !== this.#documentWorkspaceContext?.getUnique()) return;
		if (event.getEntityType() !== this.#documentWorkspaceContext.getEntityType()) return;
		this.#debounceRequestUrls();
	};

	override render() {
		return html`
			<uui-box headline=${this.localize.term('general_links')}>
				${when(
					this._loading,
					() => this.#renderLoading(),
					() => this.#renderContent(),
				)}
			</uui-box>
		`;
	}

	#renderLoading() {
		return html`<div id="loader-container"><uui-loader></uui-loader></div>`;
	}

	#renderContent() {
		return html`
			${when(
				this._isNew,
				() => this.#renderNotCreated(),
				() => (this._links.length === 0 ? this.#renderNoLinks() : this.#renderLinks()),
			)}
		`;
	}

	#renderNotCreated() {
		return html`${this.#renderEmptyLink(null, null)}`;
	}

	#renderLinks() {
		return html`
			${repeat(
				this._links,
				(link) => link.url,
				(link) => this.#renderLink(link),
			)}
		`;
	}

	#renderLink(link: UmbDocumentInfoViewLink) {
		if (!link.url) {
			return this.#renderEmptyLink(link.culture, link.state);
		}

		return html`
			<a class="link-item" href=${link.url} target="_blank">
				<span>
					${this.#renderLinkCulture(link.culture)}
					<span>${link.url}</span>
				</span>
				<uui-icon name="icon-out"></uui-icon>
			</a>
		`;
	}

	#renderNoLinks() {
		return html` ${this._variantOptions?.map((variantOption) =>
			this.#renderEmptyLink(variantOption.culture, variantOption.variant?.state),
		)}`;
	}

	#renderEmptyLink(culture: string | null, state: DocumentVariantStateModel | null | undefined) {
		return html`<div class="link-item">
			<span>
				${this.#renderLinkCulture(culture)}
				<em><umb-localize key=${this.#getStateLocalizationKey(state)}></umb-localize></em>
			</span>
		</div>`;
	}

	#renderLinkCulture(culture: string | null) {
		if (!culture) return nothing;
		if (this._links.length === 1) return nothing;
		return html`<span class="culture">${culture}</span>`;
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();

		this.#eventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadRequest as unknown as EventListener,
		);
	}

	static override styles = [
		css`
			uui-box {
				--uui-box-default-padding: 0;
			}

			#loader-container {
				display: flex;
				justify-content: center;
				align-items: center;
				padding: var(--uui-size-space-2);
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
