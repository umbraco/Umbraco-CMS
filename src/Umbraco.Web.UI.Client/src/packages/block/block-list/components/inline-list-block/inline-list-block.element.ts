import { UMB_BLOCK_LIST_CONTEXT } from '../../index.js';
import { UMB_BLOCK_WORKSPACE_ALIAS, UMB_BLOCK_WORKSPACE_CONTEXT } from '../../../block/index.js';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../../../block/workspace/views/edit/block-workspace-view-edit-no-router.element.js';

/**
 * @element umb-inline-list-block
 */
@customElement('umb-inline-list-block')
export class UmbInlineListBlockElement extends UmbLitElement {
	#blockContext?: typeof UMB_BLOCK_LIST_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#contentUdi?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_LIST_CONTEXT, (blockContext) => {
			this.#blockContext = blockContext;
			this.observe(
				this.#blockContext.contentUdi,
				(contentUdi) => {
					this.#contentUdi = contentUdi;
					this.#load();
				},
				'observeContentUdi',
			);
		});
		this.observe(umbExtensionsRegistry.getByTypeAndAlias('workspace', UMB_BLOCK_WORKSPACE_ALIAS), (manifest) => {
			if (manifest) {
				createExtensionApi(manifest, [this, { manifest: manifest }]).then((context) => {
					if (context) {
						this.#workspaceContext = context as typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
						this.#load();
						this.#workspaceContext.content.createPropertyDatasetContext(this);

						new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [
							this,
							this.#workspaceContext,
						]);
					}
				});
			}
		});
	}

	#load() {
		if (!this.#workspaceContext || !this.#contentUdi) return;
		this.#workspaceContext.load(this.#contentUdi);
	}

	render() {
		return html`<umb-block-workspace-view-edit-no-router></umb-block-workspace-view-edit-no-router>`;
	}
}

export default UmbInlineListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-inline-list-block': UmbInlineListBlockElement;
	}
}
