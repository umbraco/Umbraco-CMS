import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, state } from 'lit/decorators.js';
import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context';
import type { UmbInputTemplateElement } from '../../../../../templating/templates/components/input-template/input-template.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-type-workspace-view-templates')
export class UmbDocumentTypeWorkspaceViewTemplatesElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _defaultTemplateId?: string | null;

	@state()
	private _allowedTemplateIds?: Array<string>;

	constructor() {
		super();
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext as UmbDocumentTypeWorkspaceContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.defaultTemplateId,
			(defaultTemplateId) => (this._defaultTemplateId = defaultTemplateId)
		);
		this.observe(this.#workspaceContext.allowedTemplateIds, (allowedTemplateIds) => {
			console.log('allowedTemplateIds', allowedTemplateIds);
			this._allowedTemplateIds = allowedTemplateIds;
		});
	}

	#templateInputChange(e: CustomEvent) {
		console.log('change', e);
		// save new allowed ids
		const input = e.target as UmbInputTemplateElement;
		const idsWithoutRoot = input.selectedIds.filter((id) => id !== null) as Array<string>;
		this.#workspaceContext?.setAllowedTemplateIds(idsWithoutRoot);
		this.#workspaceContext?.setDefaultTemplateId(input.defaultId);
	}

	render() {
		return html`<uui-box headline="Templates">
			<umb-workspace-property-layout alias="Templates" label="Allowed Templates">
				<div slot="description">Choose which templates editors are allowed to use on content of this type</div>
				<div id="templates" slot="editor">
					<umb-input-template
						.defaultId=${this._defaultTemplateId}
						.selectedIds=${this._allowedTemplateIds}
						@change=${this.#templateInputChange}></umb-input-template>
				</div>
			</umb-workspace-property-layout>
		</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			#templates {
				text-align: center;
			}

			#template-card-wrapper {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: stretch;
			}

			umb-workspace-property-layout {
				border-top: 1px solid var(--uui-color-border);
			}
			umb-workspace-property-layout:first-child {
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
