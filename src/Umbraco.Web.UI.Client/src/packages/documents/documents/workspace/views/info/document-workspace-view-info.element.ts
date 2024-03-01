import { TimeOptions } from './utils.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import {
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_TEMPLATE_PICKER_MODAL,
	UMB_WORKSPACE_MODAL,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';
import './document-workspace-view-info-history.element.js';
import './document-workspace-view-info-reference.element.js';
import type { UmbDocumentWorkspaceContext } from '@umbraco-cms/backoffice/document';
import { DocumentVariantStateModel, type DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	type UmbDocumentTypeDetailModel,
	UmbDocumentTypeDetailRepository,
} from '@umbraco-cms/backoffice/document-type';
import { UmbTemplateDetailRepository } from '@umbraco-cms/backoffice/template';

@customElement('umb-document-workspace-view-info')
export class UmbDocumentWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _nodeName = '';

	@state()
	private _documentUnique = '';

	@state()
	private _urls?: Array<DocumentUrlInfoModel>;

	@state()
	private _createDate = 'Unknown';

	@state()
	private _state = DocumentVariantStateModel.DRAFT;

	/**Document Type */
	@state()
	private _documentTypeUnique = '';

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _editDocumentTypePath = '';

	@state()
	private _allowedTemplates?: UmbDocumentTypeDetailModel['allowedTemplates'];

	/**Template */
	@state()
	private _templateUnique = '';

	@state()
	private _templateName?: string;

	@state()
	private _editTemplatePath = '';

	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#workspaceContext?: UmbDocumentWorkspaceContext;

	#templateRepository = new UmbTemplateDetailRepository(this);
	#documentTypeRepository = new UmbDocumentTypeDetailRepository(this);

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
			this.#workspaceContext = nodeContext as UmbDocumentWorkspaceContext;
			this._documentTypeUnique = this.#workspaceContext.getContentTypeId()!;
			this.#getData();
			this._observeContent();
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManagerContext) => {
			this.#modalManagerContext = modalManagerContext;
		});
	}

	async #getData() {
		const { data } = await this.#documentTypeRepository.requestByUnique(this._documentTypeUnique);
		this._documentTypeName = data?.name;
		this._documentTypeIcon = data?.icon;
		this._allowedTemplates = data?.allowedTemplates;
	}

	private _observeContent() {
		if (!this.#workspaceContext) return;

		this._nodeName = 'TBD, with variants this is not as simple.';

		this.observe(this.#workspaceContext.urls, (urls) => {
			this._urls = urls;
		});

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._documentUnique = unique!;
			},
			'_documentUnique',
		);

		this.observe(
			this.#workspaceContext.templateId,
			async (templateUnique) => {
				this._templateUnique = templateUnique!;
				if (!templateUnique) return;

				const { data } = await this.#templateRepository.requestByUnique(templateUnique!);
				this._templateName = data?.name;

				new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
					.addAdditionalPath('template')
					.onSetup(() => {
						return { data: { entityType: 'template', preset: {} } };
					})
					.observeRouteBuilder((routeBuilder) => {
						this._editTemplatePath = routeBuilder({});
					});
			},
			'_templateUnique',
		);

		/** TODO: Doubt this is the right way to get the create date... */
		this.observe(this.#workspaceContext.variants, (variants) => {
			this._createDate = Array.isArray(variants) ? variants[0].createDate || 'Unknown' : 'Unknown';
			if (variants[0].state) this._state = variants[0].state;
		});
	}

	#renderStateTag() {
		switch (this._state) {
			case DocumentVariantStateModel.DRAFT:
				return html`<uui-tag look="secondary" label=${this.localize.term('content_unpublished')}
					>${this.localize.term('content_unpublished')}</uui-tag
				>`;
			case DocumentVariantStateModel.PUBLISHED:
				return html`<uui-tag color="positive" look="primary" label=${this.localize.term('content_published')}
					>${this.localize.term('content_published')}</uui-tag
				>`;
			case DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES:
				return html`<uui-tag
					color="positive"
					look="primary"
					label=${this.localize.term('content_publishedPendingChanges')}
					>${this.localize.term('content_published')}</uui-tag
				>`;
			default:
				return html`<uui-tag look="primary" label=${this.localize.term('content_published')}
					>${this.localize.term('content_published')}</uui-tag
				>`;
		}
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
				<strong><umb-localize key="content_publishStatus">Publication Status</umb-localize></strong>
				<span> ${this.#renderStateTag()} </span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_createDate">Created</umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_documentType">Document Type</umb-localize></strong>
				<uui-ref-node-document-type
					href=${this._editDocumentTypePath + 'edit/' + this._documentTypeUnique}
					name=${ifDefined(this._documentTypeName)}>
					<uui-icon slot="icon" name=${ifDefined(this._documentTypeIcon)}></uui-icon>
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_template">Template</umb-localize></strong>
				${this._templateUnique
					? html`<uui-ref-node
							name=${ifDefined(this._templateName)}
							href=${this._editTemplatePath + 'edit/' + this._templateUnique}>
							<uui-icon slot="icon" name="icon-newspaper"></uui-icon>
							<uui-button
								slot="actions"
								label=${this.localize.term('general_edit')}
								@click=${this.#openTemplatePicker}></uui-button>
					  </uui-ref-node>`
					: html`<uui-button
							label=${this.localize.term('general_edit')}
							@click=${this.#openTemplatePicker}></uui-button>`}
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._documentUnique}</span>
			</div>
		`;
	}

	async #openTemplatePicker() {
		const modal = this.#modalManagerContext?.open(UMB_TEMPLATE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				multiple: false,
				pickableFilter: (template) =>
					this._allowedTemplates?.find((allowed) => template.unique === allowed.id) ? true : false,
			},
			value: {
				selection: [this._templateUnique],
			},
		});

		const result = await modal?.onSubmit().catch(() => undefined);

		if (!result?.selection.length) return;

		const templateUnique = result.selection[0];

		if (!templateUnique) return;

		this.#workspaceContext?.setTemplate(templateUnique);
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
