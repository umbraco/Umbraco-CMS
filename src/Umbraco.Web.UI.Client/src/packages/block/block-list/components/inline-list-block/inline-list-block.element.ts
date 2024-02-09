import { UMB_BLOCK_LIST_ENTRY_CONTEXT } from '../../index.js';
import type { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../../block/index.js';
import { UMB_BLOCK_WORKSPACE_ALIAS } from '../../../block/index.js';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../../../block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 * @element umb-inline-list-block
 */
@customElement('umb-inline-list-block')
export class UmbInlineListBlockElement extends UmbLitElement {
	#blockContext?: typeof UMB_BLOCK_LIST_ENTRY_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#contentUdi?: string;

	@property({ type: String })
	label?: string;

	@state()
	_isOpen = false;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_LIST_ENTRY_CONTEXT, (blockContext) => {
			this.#blockContext = blockContext;
			this.observe(
				this.#blockContext.unique,
				(contentUdi) => {
					this.#contentUdi = contentUdi;
					this.#load();
				},
				'observeContentUdi',
			);
		});
		this.observe(umbExtensionsRegistry.byTypeAndAlias('workspace', UMB_BLOCK_WORKSPACE_ALIAS), (manifest) => {
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
		return html` <uui-box>
			<button
				slot="header"
				id="accordion-button"
				@click=${() => {
					this._isOpen = !this._isOpen;
				}}>
				<uui-icon name="icon-document"></uui-icon>
				<uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
				<span>${this.label}</span>
			</button>
			${this._isOpen === true
				? html`<umb-block-workspace-view-edit-content-no-router></umb-block-workspace-view-edit-content-no-router>`
				: ''}
		</uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#accordion-button {
				display: flex;
				text-align: left;
				align-items: center;
				justify-content: flex-start;
				width: 100%;
				border: none;
				background: none;
				padding: 0;
			}
		`,
	];
}

export default UmbInlineListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-inline-list-block': UmbInlineListBlockElement;
	}
}
