import { UMB_BLOCK_TYPE_WORKSPACE_CONTEXT } from './block-type-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';
import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-block-type-workspace-editor')
export class UmbBlockTypeWorkspaceEditorElement extends UmbLitElement {
	#itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(this, UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS);

	#workspaceContext?: typeof UMB_BLOCK_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_name?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.createPropertyDatasetContext(this);
			this.observe(this.#workspaceContext.unique, (unique) => {
				if (unique) {
					this.#itemManager.setUniques([unique]);
				}
			});
		});

		this.observe(this.#itemManager.items, (items) => {
			const item = items[0];
			if (item) {
				this._name = item.name;
			}
		});
	}

	override render() {
		return html`
			<umb-workspace-editor headline=${this.localize.term('blockEditor_blockConfigurationOverlayTitle', [this._name])}>
			</umb-workspace-editor>
		`;
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

export default UmbBlockTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-workspace-editor': UmbBlockTypeWorkspaceEditorElement;
	}
}
