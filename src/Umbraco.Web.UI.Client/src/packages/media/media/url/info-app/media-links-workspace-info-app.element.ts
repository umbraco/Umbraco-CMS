import { UmbMediaUrlRepository } from '../repository/index.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import type { UmbMediaUrlModel } from '../repository/types.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import type { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';

interface UmbMediaInfoViewLink {
	url: string | undefined;
}

@customElement('umb-media-links-workspace-info-app')
export class UmbMediaLinksWorkspaceInfoAppElement extends UmbLitElement {
	#mediaUrlRepository = new UmbMediaUrlRepository(this);

	@state()
	private _isNew = false;

	@state()
	private _unique?: string;

	@state()
	private _loading = false;

	@state()
	private _links: Array<UmbMediaInfoViewLink> = [];

	#urls: Array<UmbMediaUrlModel> = [];

	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			if (context) {
				this.observe(
					observeMultiple([context.isNew, context.unique]),
					([isNew, unique]) => {
						if (!unique) return;
						this._isNew = isNew === true;

						if (unique !== this._unique) {
							this._unique = unique;
							this.#requestUrls();
						}
					},
					'observeWorkspaceState',
				);
			} else {
				this.removeUmbControllerByAlias('observeWorkspaceState');
			}
		});
	}

	#setLinks() {
		const links: Array<UmbMediaInfoViewLink> = this.#urls.map((u) => {
			const url = u.url;
			return { url };
		});

		this._links = links;
	}

	async #requestUrls() {
		if (this._isNew) return;
		if (!this._unique) return;

		this._loading = true;
		this.#urls = [];

		const { data } = await this.#mediaUrlRepository.requestItems([this._unique]);

		if (data?.length) {
			this.#urls = data;
			this.#setLinks();
		}

		this._loading = false;
	}

	#debounceRequestUrls = debounce(() => this.#requestUrls(), 50);

	#onReloadRequest = () => {
		this.#debounceRequestUrls();
	};

	override render() {
		return html`
			<umb-workspace-info-app-layout headline="#general_links">
				${when(
					this._loading,
					() => this.#renderLoading(),
					() => this.#renderContent(),
				)}
			</umb-workspace-info-app-layout>
		`;
	}

	#renderLoading() {
		return html`<div id="loader-container"><uui-loader></uui-loader></div>`;
	}

	#openSvg(imagePath: string) {
		const popup = window.open('', '_blank');
		if (!popup) return;

		const html = `<!doctype html>
<body style="background-image: linear-gradient(45deg, #ccc 25%, transparent 25%), linear-gradient(135deg, #ccc 25%, transparent 25%), linear-gradient(45deg, transparent 75%, #ccc 75%), linear-gradient(135deg, transparent 75%, #ccc 75%); background-size:30px 30px; background-position:0 0, 15px 0, 15px -15px, 0px 15px;">
	<img src="${imagePath}"/>
	<script>history.pushState(null, null, "${window.location.href}");</script>
</body>`;

		popup.document.open();
		popup.document.write(html);
		popup.document.close();
	}

	#renderContent() {
		if (this._links.length) {
			return html`
				${repeat(
					this._links,
					(item) => item.url,
					(item) => this.#renderLinkItem(item),
				)}
			`;
		} else {
			return html`
				<div class="link-item">
					<span class="link-content italic"><umb-localize key="content_noMediaLink"></umb-localize></span>
				</div>
			`;
		}
	}

	#renderLinkItem(item: UmbMediaInfoViewLink) {
		if (!item.url) return nothing;
		const ext = item.url.split(/[#?]/)[0].split('.').pop()?.trim();
		if (ext === 'svg') {
			return html`
				<a href="#" target="_blank" class="link-item with-href" @click=${() => this.#openSvg(item.url!)}>
					<span class="link-content">${item.url}</span>
					<uui-icon name="icon-out"></uui-icon>
				</a>
			`;
		} else {
			return html`
				<a href=${item.url} target="_blank" class="link-item with-href">
					<span class="link-content">${item.url}</span>
					<uui-icon name="icon-out"></uui-icon>
				</a>
			`;
		}
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

			#link-section {
				display: flex;
				flex-direction: column;
				text-align: left;
			}

			.link-item {
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				display: grid;
				grid-template-columns: 1fr auto;
				gap: var(--uui-size-6);
				color: inherit;
				text-decoration: none;
				word-break: break-all;
			}

			.link-language {
				color: var(--uui-color-divider-emphasis);
			}

			.link-content.italic {
				font-style: italic;
			}

			.link-item uui-icon {
				margin-right: var(--uui-size-space-2);
				vertical-align: middle;
			}

			.link-item.with-href {
				cursor: pointer;
			}

			.link-item.with-href:hover {
				background: var(--uui-color-divider);
			}
		`,
	];
}

export default UmbMediaLinksWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-links-workspace-info-app': UmbMediaLinksWorkspaceInfoAppElement;
	}
}
