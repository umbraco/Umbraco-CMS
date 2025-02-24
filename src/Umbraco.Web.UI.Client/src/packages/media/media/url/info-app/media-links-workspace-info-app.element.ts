import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import type { MediaUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-links-workspace-info-app')
export class UmbMediaLinksWorkspaceInfoAppElement extends UmbLitElement {
	@state()
	private _urls?: Array<MediaUrlInfoModel>;

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUrls();
		});
	}

	#observeUrls() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.urls,
			(urls) => {
				this._urls = urls;
			},
			'__urls',
		);
	}

	protected override render() {
		return html`<umb-workspace-info-app-layout headline="#general_links">
			${this.#renderLinksSection()}
		</umb-workspace-info-app-layout> `;
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

	#renderLinksSection() {
		if (this._urls && this._urls.length) {
			return html`
				${repeat(
					this._urls,
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

	#renderLinkItem(item: MediaUrlInfoModel) {
		const ext = item.url.split(/[#?]/)[0].split('.').pop()?.trim();
		if (ext === 'svg') {
			return html`
				<a href="#" target="_blank" class="link-item with-href" @click=${() => this.#openSvg(item.url)}>
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
