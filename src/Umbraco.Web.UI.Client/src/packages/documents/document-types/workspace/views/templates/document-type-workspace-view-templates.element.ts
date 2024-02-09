import type { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import type { UmbInputTemplateElement } from '../../../../../templating/templates/components/input-template/input-template.element.js';
import '../../../../../templating/templates/components/input-template/input-template.element.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-type-workspace-view-templates')
export class UmbDocumentTypeWorkspaceViewTemplatesElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _defaultTemplateId: string | null = null;

	@state()
	private _allowedTemplateIds?: Array<string>;

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext as UmbDocumentTypeWorkspaceContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.defaultTemplate,
			(defaultTemplate) => {
				const oldValue = this._defaultTemplateId;
				this._defaultTemplateId = defaultTemplate ? defaultTemplate.id : null;
				this.requestUpdate('_defaultTemplateId', oldValue);
			},
			'defaultTemplate',
		);
		this.observe(
			this.#workspaceContext.allowedTemplateIds,
			(allowedTemplateIds) => {
				const oldValue = this._allowedTemplateIds;
				this._allowedTemplateIds = allowedTemplateIds?.map((template) => template.id);
				this.requestUpdate('_allowedTemplateIds', oldValue);
			},
			'allowedTemplateIds',
		);
	}

	#templateInputChange(e: CustomEvent) {
		// save new allowed ids
		const input = e.target as UmbInputTemplateElement;
		const idsWithoutRoot =
			input.selectedIds
				?.filter((id) => id !== null)
				.map((id) => {
					return { id };
				}) ?? [];
		this.#workspaceContext?.setAllowedTemplateIds(idsWithoutRoot);
		this.#workspaceContext?.setDefaultTemplate({ id: input.defaultUnique });
	}

	render() {
		return html`<uui-box headline="Templates">
			<umb-property-layout alias="Templates" label="Allowed Templates">
				<div slot="description">Choose which templates editors are allowed to use on content of this type</div>
				<div id="templates" slot="editor">
					<umb-input-template
						.defaultUnique=${this._defaultTemplateId ?? ''}
						.selectedIds=${this._allowedTemplateIds}
						@change=${this.#templateInputChange}></umb-input-template>
				</div>
			</umb-property-layout>
		</uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
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
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewTemplatesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-templates': UmbDocumentTypeWorkspaceViewTemplatesElement;
	}
}
