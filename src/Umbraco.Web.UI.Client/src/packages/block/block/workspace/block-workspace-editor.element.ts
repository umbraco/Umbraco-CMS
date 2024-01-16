import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	//
	#itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		(x) => x.id,
	);

	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	@property({ type: String, attribute: false })
	workspaceAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.content.createPropertyDatasetContext(this);
		});
	}

	render() {
		return this.workspaceAlias
			? html` <umb-workspace-editor alias=${this.workspaceAlias} headline=${'BLOCK EDITOR'}> </umb-workspace-editor> `
			: '';
	}

	static styles = [
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

export default UmbBlockWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-editor': UmbBlockWorkspaceEditorElement;
	}
}
