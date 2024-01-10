import { TimeOptions } from './utils.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import './document-workspace-view-info-history.element.js';
import './document-workspace-view-info-reference.element.js';
import { UmbDocumentWorkspaceContext } from '@umbraco-cms/backoffice/document';
import { ContentUrlInfoModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbCurrentUserContext } from '@umbraco-cms/backoffice/current-user';

@customElement('umb-document-workspace-view-info')
export class UmbDocumentWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _nodeName = '';

	@state()
	private _documentTypeId = '';

	@state()
	private _documentUnique = '';

	private _workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _editDocumentTypePath = '';

	@state()
	private _urls?: Array<ContentUrlInfoModel>;

	@state()
	private _createDate = 'Unknown';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document-type')
			.onSetup(() => {
				return { data: { entityType: 'document-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentTypePath = routeBuilder({});
			});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		this._nodeName = 'TBD, with variants this is not as simple.';

		this._documentTypeId = (this._workspaceContext as UmbDocumentWorkspaceContext).getContentTypeId()!;

		this.observe((this._workspaceContext as UmbDocumentWorkspaceContext).urls, (urls) => {
			this._urls = urls;
		});

		this.observe((this._workspaceContext as UmbDocumentWorkspaceContext).unique, (unique) => {
			this._documentUnique = unique!;
		});

		/** TODO: Doubt this is the right way to get the create date... */
		this.observe((this._workspaceContext as UmbDocumentWorkspaceContext).variants, (variants) => {
			this._createDate = Array.isArray(variants) ? variants[0].createDate : 'Unknown';
		});
	}

	render() {
		return html`<div class="container">
				<uui-box headline=${this.localize.term('general_links')} style="--uui-box-default-padding: 0;">
					<div id="link-section">${this.#renderLinksSection()}</div>
				</uui-box>

				<umb-document-workspace-view-info-reference
					.documentUnique=${this._documentUnique}></umb-document-workspace-view-info-reference>

				<umb-document-workspace-view-info-history
					.documentUnique=${this._documentUnique}></umb-document-workspace-view-info-history>
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
				<strong><umb-localize key="content_documentType"></umb-localize></strong>
				<uui-button
					look="secondary"
					href=${this._editDocumentTypePath + 'edit/' + this._documentTypeId}
					label=${this.localize.term('general_edit')}></uui-button>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_template"></umb-localize></strong>
				<uui-button look="secondary" label="Template picker TODO"></uui-button>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id"></umb-localize></strong>
				<span>${this._documentTypeId}</span>
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

export default UmbDocumentWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-info': UmbDocumentWorkspaceViewInfoElement;
	}
}
