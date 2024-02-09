import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-ref-grid-block
 */
@customElement('umb-ref-grid-block')
export class UmbRefGridBlockElement extends UmbLitElement {
	//
	@property({ type: String })
	label?: string;

	@state()
	_workspaceEditPath?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
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
		return html`<uui-ref-node standalone .name=${this.label ?? ''}></uui-ref-node>`;
	}

	static styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
		`,
	];
}

export default UmbRefGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-grid-block': UmbRefGridBlockElement;
	}
}
