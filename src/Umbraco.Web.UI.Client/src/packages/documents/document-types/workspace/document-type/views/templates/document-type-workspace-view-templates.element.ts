import type { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from '../../document-type-workspace.context-token.js';
import type { UmbInputTemplateElement } from '@umbraco-cms/backoffice/template';
import { css, html, customElement, state, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

// TODO: This is needed to register the <umb-input-template> element, but it should be done in a better way without importing the whole module. [JOV]
import '@umbraco-cms/backoffice/template';

@customElement('umb-document-type-workspace-view-templates')
export class UmbDocumentTypeWorkspaceViewTemplatesElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _isElementType = false;

	@state()
	private _defaultTemplateId: string | null = null;

	@state()
	private _allowedTemplateIds?: Array<string>;

	// Restricted until the server confirms it is not in production runtime mode (safe default).
	@state()
	private _isRestricted = true;

	constructor() {
		super();
		// In production runtime mode the schema is read-only. Making the whole view inert disables every
		// input/toggle/button at once without wiring each one, and the dimmed styling signals it is read-only.
		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(
				context?.isProductionMode,
				(isProductionMode) => {
					this._isRestricted = isProductionMode !== false;
					this.inert = this._isRestricted;
				},
				'_observeProductionMode',
			);
		});

		this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext as UmbDocumentTypeWorkspaceContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.isElement,
			(isElement) => {
				this._isElementType = isElement ?? false;
			},
			'_observeIsElement',
		);

		this.observe(
			this.#workspaceContext.defaultTemplate,
			(defaultTemplate) => {
				const oldValue = this._defaultTemplateId;
				this._defaultTemplateId = defaultTemplate ? defaultTemplate.id : null;
				this.requestUpdate('_defaultTemplateId', oldValue);
			},
			'_observeDefaultTemplate',
		);
		this.observe(
			this.#workspaceContext.allowedTemplateIds,
			(allowedTemplateIds) => {
				const oldValue = this._allowedTemplateIds;
				this._allowedTemplateIds = allowedTemplateIds?.map((template) => template.id);
				this.requestUpdate('_allowedTemplateIds', oldValue);
			},
			'_observeAllowedTemplateIds',
		);
	}

	#templateInputChange(e: CustomEvent) {
		// save new allowed ids
		const input = e.target as UmbInputTemplateElement;
		const idsWithoutRoot =
			input.selection
				?.filter((id) => id !== null)
				.map((id) => {
					return { id };
				}) ?? [];
		this.#workspaceContext?.setAllowedTemplateIds(idsWithoutRoot);
		this.#workspaceContext?.setDefaultTemplate(input.defaultUnique ? { id: input.defaultUnique } : null);
	}

	#renderProductionModeNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box id="production-mode-notice">
				<div class="notice">
					<umb-icon name="icon-info"></umb-icon>
					<div>
						<strong><umb-localize key="general_productionMode">Production Mode</umb-localize></strong>
						<p><umb-localize key="general_runtimeModeProductionSchema"></umb-localize></p>
					</div>
				</div>
			</uui-box>
		`;
	}

	override render() {
		return html`
			${this.#renderProductionModeNotice()} ${this._isElementType ? this.#renderUnsupported() : this.#renderTemplates()}
		`;
	}

	#renderUnsupported() {
		return html`
			<div class="empty-state">
				<h2>
					<umb-localize key="contentTypeEditor_elementDoesNotSupport">
						This is not applicable for an Element type.
					</umb-localize>
				</h2>
			</div>
		`;
	}

	#renderTemplates() {
		return html`
			<uui-box headline=${this.localize.term('treeHeaders_templates')}>
				${when(
					this.#workspaceContext?.createTemplateMode,
					() => html`<p><em>The default template will be created once this document type has been saved.</em></p>`,
				)}
				<umb-property-layout alias="Templates" label=${this.localize.term('contentTypeEditor_allowedTemplatesHeading')}>
					<div slot="description">${this.localize.term('contentTypeEditor_allowedTemplatesDescription')}</div>
					<div id="templates" slot="editor">
						<umb-input-template
							.defaultUnique=${this._defaultTemplateId ?? ''}
							.selection=${this._allowedTemplateIds}
							@change=${this.#templateInputChange}>
						</umb-input-template>
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host([inert]) > :not(#production-mode-notice) {
				opacity: 0.6;
			}
			#production-mode-notice {
				--uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);
				margin-bottom: var(--uui-size-layout-1);
			}
			#production-mode-notice .notice {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: flex-start;
			}
			#production-mode-notice umb-icon {
				flex: 0 0 auto;
				font-size: var(--uui-size-6);
				margin-top: 2px;
				color: var(--uui-color-warning-standalone, #f0ac00);
			}
			#production-mode-notice p {
				margin: var(--uui-size-space-2) 0 0;
			}
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			#templates {
				text-align: center;
			}

			#template-card-wrapper {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: stretch;
			}

			umb-property-layout {
				border-top: 1px solid var(--uui-color-border);
			}
			umb-property-layout:first-child {
				padding-top: 0;
				border: none;
			}
			.empty-state {
				display: flex;
				justify-content: space-around;
				flex-direction: column;
				align-items: center;
			}
			.empty-state h2 {
				color: var(--uui-color-border-emphasis);
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewTemplatesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-templates': UmbDocumentTypeWorkspaceViewTemplatesElement;
	}
}
