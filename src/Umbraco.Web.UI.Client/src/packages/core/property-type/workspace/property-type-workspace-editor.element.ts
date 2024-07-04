import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from './property-type-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';
import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-property-type-workspace-editor')
export class UmbPropertyTypeWorkspaceEditorElement extends UmbLitElement {
	//
	#itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		(x) => x.unique,
	);

	#workspaceContext?: typeof UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_name?: string;

	@property({ type: String, attribute: false })
	workspaceAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	override render() {
		return this.workspaceAlias
			? html`
					<umb-workspace-editor
						alias=${this.workspaceAlias}
						headline=${this.localize.term('blockEditor_blockConfigurationOverlayTitle', [this._name])}>
					</umb-workspace-editor>
				`
			: '';
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbPropertyTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-workspace-editor': UmbPropertyTypeWorkspaceEditorElement;
	}
}
