import { TimeOptions } from './utils.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import './media-workspace-view-info-history.element.js';
import './media-workspace-view-info-reference.element.js';
import type { UmbMediaWorkspaceContext } from '@umbraco-cms/backoffice/media';
import type { MediaUrlInfoModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-media-workspace-view-info')
export class UmbMediaWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _nodeName = '';

	@state()
	private _mediaTypeId = '';

	@state()
	private _mediaUnique = '';

	private _workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _editMediaTypePath = '';

	@state()
	private _urls?: Array<MediaUrlInfoModel>;

	@state()
	private _createDate = 'Unknown';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media-type')
			.onSetup(() => {
				return { data: { entityType: 'media-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaTypePath = routeBuilder({});
			});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		this._nodeName = 'TBD, with variants this is not as simple.';

		this._mediaTypeId = (this._workspaceContext as UmbMediaWorkspaceContext).getContentTypeId()!;

		this.observe((this._workspaceContext as UmbMediaWorkspaceContext).urls, (urls) => {
			this._urls = urls;
		});

		this.observe((this._workspaceContext as UmbMediaWorkspaceContext).unique, (unique) => {
			this._mediaUnique = unique!;
		});

		/** TODO: Doubt this is the right way to get the create date... */
		this.observe((this._workspaceContext as UmbMediaWorkspaceContext).variants, (variants) => {
			this._createDate = Array.isArray(variants) ? variants[0].createDate || 'Unknown' : 'Unknown';
		});
	}

	render() {
		return html`<div class="container">
				<uui-box headline=${this.localize.term('general_links')} style="--uui-box-default-padding: 0;">
					<div id="link-section">${this.#renderLinksSection()}</div>
				</uui-box>

				<umb-media-workspace-view-info-reference
					.mediaUnique=${this._mediaUnique}></umb-media-workspace-view-info-reference>

				<umb-media-workspace-view-info-history
					.mediaUnique=${this._mediaUnique}></umb-media-workspace-view-info-history>
			</div>
			<div class="container">
				<uui-box headline="General" id="general-section">${this.#renderGeneralSection()}</uui-box>
			</div>`;
	}

	#renderLinksSection() {
		/** TODO Make sure link section is completed */
		if (this._urls && this._urls.length) {
			return html`
				${repeat(
					this._urls,
					(url) => url.culture,
					(url) => html`
						<a href=${url.url} target="_blank" class="link-item with-href">
							<span class="link-language">${url.culture}</span>
							<span class="link-content"> ${url.url}</span>
							<uui-icon name="icon-out"></uui-icon>
						</a>
					`,
				)}
			`;
		} else {
			return html`<div class="link-item">
				<span class="link-language">en-EN</span>
				<span class="link-content italic"><umb-localize key="content_parentNotPublishedAnomaly"></umb-localize></span>
			</div>`;
		}
	}

	#renderGeneralSection() {
		return html`
			<div class="general-item">
				<strong>${this.localize.term('content_publishStatus')}</strong>
				<span>
					<uui-tag color="positive" look="primary" label=${this.localize.term('content_published')}>
						<umb-localize key="content_published"></umb-localize>
					</uui-tag>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_createDate"></umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_mediaType"></umb-localize></strong>
				<uui-button
					look="secondary"
					href=${this._editMediaTypePath + 'edit/' + this._mediaTypeId}
					label=${this.localize.term('general_edit')}></uui-button>
			</div>
		`;
	}

	static styles = [
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
				grid-template-columns: auto 1fr auto;
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
