import { UMB_BLOCK_ENTRY_CONTEXT, type UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-ref-rte-block
 */
@customElement('umb-ref-rte-block')
export class UmbRefRteBlockElement extends UmbLitElement {
	//
	@property({ type: String })
	label?: string;

	@property({ type: String })
	icon?: string;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@state()
	_workspaceEditPath?: string;

	constructor() {
		super();

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

	override render() {
		return html`
			<uui-ref-node standalone href=${this._workspaceEditPath ?? '#'}>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			</uui-ref-node>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
			}
		`,
	];
}

export default UmbRefRteBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-rte-block': UmbRefRteBlockElement;
	}
}
