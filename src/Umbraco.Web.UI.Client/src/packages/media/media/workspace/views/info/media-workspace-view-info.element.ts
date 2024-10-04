import { TimeOptions } from './utils.js';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbMediaTypeItemModel } from '@umbraco-cms/backoffice/media-type';
import { UMB_MEDIA_TYPE_ENTITY_TYPE, UmbMediaTypeItemRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/media';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { MediaUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';

// import of local components
import './media-workspace-view-info-history.element.js';
import './media-workspace-view-info-reference.element.js';

@customElement('umb-media-workspace-view-info')
export class UmbMediaWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _mediaTypeUnique: string | undefined = undefined;

	@state()
	private _mediaTypeName?: UmbMediaTypeItemModel['name'];

	@state()
	private _mediaTypeIcon?: UmbMediaTypeItemModel['icon'];

	@state()
	private _editMediaTypePath = '';

	@state()
	private _mediaUnique = '';

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;

	#mediaTypeItemRepository = new UmbMediaTypeItemRepository(this);

	@state()
	private _urls?: Array<MediaUrlInfoModel>;

	@state()
	private _createDate = 'Unknown';

	@state()
	private _updateDate = 'Unknown';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media-type')
			.onSetup(() => {
				return { data: { entityType: UMB_MEDIA_TYPE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaTypePath = routeBuilder({});
			});

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._mediaTypeUnique = this.#workspaceContext.getContentTypeId()!;
			this.#getData();
			this.#observeContent();
		});
	}

	async #getData() {
		if (!this._mediaTypeUnique) throw new Error('Media type unique is not set');
		const { data } = await this.#mediaTypeItemRepository.requestItems([this._mediaTypeUnique]);
		this._mediaTypeName = data?.[0].name;
		this._mediaTypeIcon = data?.[0].icon;
	}

	#observeContent() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.urls,
			(urls) => {
				this._urls = urls;
			},
			'__urls',
		);

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._mediaUnique = unique!;
			},
			'_mediaUnique',
		);

		/** TODO: Doubt this is the right way to get the create date... */
		this.observe(this.#workspaceContext.variants, (variants) => {
			this._createDate = Array.isArray(variants) ? variants[0].createDate || 'Unknown' : 'Unknown';
			this._updateDate = Array.isArray(variants) ? variants[0].updateDate || 'Unknown' : 'Unknown';
		});
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

	override render() {
		return html`
			<div class="container">
				<uui-box headline=${this.localize.term('general_links')} style="--uui-box-default-padding: 0;">
					<div id="link-section">${this.#renderLinksSection()}</div>
				</uui-box>

				<umb-media-workspace-view-info-reference
					.mediaUnique=${this._mediaUnique}></umb-media-workspace-view-info-reference>

				<umb-media-workspace-view-info-history
					.mediaUnique=${this._mediaUnique}></umb-media-workspace-view-info-history>
			</div>
			<div class="container">
				<uui-box headline=${this.localize.term('general_general')} id="general-section"
					>${this.#renderGeneralSection()}</uui-box
				>
			</div>
		`;
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

	#renderGeneralSection() {
		return html`
			<div class="general-item">
				<strong><umb-localize key="content_createDate"></umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_updateDate"></umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._updateDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_mediaType">Media Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${this._editMediaTypePath + 'edit/' + this._mediaTypeUnique}
					name=${ifDefined(this._mediaTypeName)}>
					${this._mediaTypeIcon ? html`<umb-icon slot="icon" name=${this._mediaTypeIcon}></umb-icon>` : nothing}
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._mediaUnique}</span>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			div.container {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}

			//General section

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}

			// Link section

			#link-section {
				display: flex;
				flex-direction: column;
				text-align: left;
			}

			.link-item {
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				display: grid;
				grid-template-columns: 1fr auto;
				gap: var(--uui-size-6);
				color: inherit;
				text-decoration: none;
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

export default UmbMediaWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-info': UmbMediaWorkspaceViewInfoElement;
	}
}
