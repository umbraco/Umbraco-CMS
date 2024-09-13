import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';

import '@umbraco-cms/backoffice/ufm';

/**
 * @element umb-ref-list-block
 */
@customElement('umb-ref-list-block')
export class UmbRefListBlockElement extends UmbLitElement {
	//
	@property({ type: String, reflect: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@state()
	_content?: UmbBlockDataType;

	@property()
	_workspaceEditPath?: string;

	constructor() {
		super();

		// UMB_BLOCK_LIST_ENTRY_CONTEXT
		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.observe(
				context.content,
				(content) => {
					this._content = content;
				},
				'observeContent',
			);

			this.observe(
				context.workspaceEditContentPath,
				(workspaceEditPath) => {
					this._workspaceEditPath = workspaceEditPath;
				},
				'observeWorkspaceEditPath',
			);
		});
	}

	override render() {
		return html`
			<uui-ref-node standalone href=${this._workspaceEditPath ?? '#'}>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this._content}></umb-ufm-render>
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
		`,
	];
}

export default UmbRefListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-list-block': UmbRefListBlockElement;
	}
}
