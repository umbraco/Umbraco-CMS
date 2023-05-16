import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import type { UUIToggleElement } from '@umbraco-ui/uui';
import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-type-workspace-view-details')
export class UmbDocumentTypeWorkspaceViewDetailsElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _variesByCulture?: boolean;
	@state()
	private _variesBySegment?: boolean;
	@state()
	private _isElement?: boolean;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext as UmbDocumentTypeWorkspaceContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.variesByCulture,
			(variesByCulture) => (this._variesByCulture = variesByCulture)
		);
		this.observe(
			this.#workspaceContext.variesBySegment,
			(variesBySegment) => (this._variesBySegment = variesBySegment)
		);
		this.observe(this.#workspaceContext.isElement, (isElement) => (this._isElement = isElement));
	}

	render() {
		return html`
			<uui-box headline="Data configuration">
				<umb-workspace-property-layout alias="VaryByCulture" label="Allow vary by culture">
					<div slot="description">Allow editors to create content of different languages.</div>
					<div slot="editor">
						<uui-toggle
							.checked=${this._variesByCulture}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setVariesByCulture((e.target as UUIToggleElement).checked);
							}}
							label="Vary by culture"></uui-toggle>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="VaryBySegments" label="Allow segmentation">
					<div slot="description">Allow editors to segment their content.</div>
					<div slot="editor">
						<uui-toggle
							.checked=${this._variesBySegment}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setVariesBySegment((e.target as UUIToggleElement).checked);
							}}
							label="Vary by segments"></uui-toggle>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="ElementType" label="Is an Element Type">
					<div slot="description">
						An Element Type is used for content instances in Property Editors, like the Block Editors.
					</div>
					<div slot="editor">
						<uui-toggle
							.checked=${this._isElement}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setIsElement((e.target as UUIToggleElement).checked);
							}}
							label="Element type"></uui-toggle>
					</div>
				</umb-workspace-property-layout>
			</uui-box>
			<uui-box headline="History cleanup">
				<umb-workspace-property-layout alias="HistoryCleanup" label="History cleanup">
					<div slot="description">
						Allow overriding the global history cleanup settings. (TODO: this ui is not working.. )
					</div>
					<div slot="editor">
						<!-- TODO: Bind this with context/data -->
						<uui-toggle .checked="${true}" label="Auto cleanup"></uui-toggle>
						<uui-label for="versions-newer-than-days">Keep all versions newer than X days</uui-label>
						<umb-property-editor-ui-number id="versions-newer-than-days"></umb-property-editor-ui-number>
						<uui-label for="latest-version-per-day-days">Keep latest version per day for X days</uui-label>
						<umb-property-editor-ui-number id="latest-version-per-day-days"></umb-property-editor-ui-number>
					</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-box {
				margin: var(--uui-size-layout-1);
			}

			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			// TODO: is this necessary?
			uui-toggle {
				display: flex;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewDetailsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-details': UmbDocumentTypeWorkspaceViewDetailsElement;
	}
}
