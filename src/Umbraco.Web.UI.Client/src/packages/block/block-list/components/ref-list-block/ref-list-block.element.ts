import { UMB_BLOCK_LIST_CONTEXT } from '../../context/block-list.context-token.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-ref-list-block
 */
@customElement('umb-ref-list-block')
export class UmbRefListBlockElement extends UmbLitElement {
	//
	@property({ type: String })
	label?: string;

	@state()
	_workspaceEditPath?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_LIST_CONTEXT, (context) => {
			this.observe(
				context.workspaceEditPath,
				(workspaceEditPath) => {
					this._workspaceEditPath = workspaceEditPath;
				},
				'observeWorkspaceEditPath',
			);
		});
	}

	render() {
		// href=${this._workspaceEditPath ?? '#'}
		return html`<uui-ref-node border .name=${this.label ?? ''}></uui-ref-node>`;
	}

	static styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
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
