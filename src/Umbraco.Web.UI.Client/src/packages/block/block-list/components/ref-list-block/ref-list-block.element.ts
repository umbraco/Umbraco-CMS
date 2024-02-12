import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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

		// UMB_BLOCK_LIST_ENTRY_CONTEXT
		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.observe(
				context.workspaceEditContentPath,
				(workspaceEditPath) => {
					this._workspaceEditPath = workspaceEditPath;
				},
				'observeWorkspaceEditPath',
			);
		});
	}

	render() {
		return html`<uui-ref-node
			standalone
			.name=${this.label ?? ''}
			href=${this._workspaceEditPath ?? '#'}></uui-ref-node>`;
	}

	static styles = [
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
