import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

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

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	@state()
	private _workspaceEditPath?: string;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.observe(
				context?.workspaceEditContentPath,
				(workspaceEditPath) => {
					this._workspaceEditPath = workspaceEditPath;
				},
				'observeWorkspaceEditPath',
			);
		});
	}

	override render() {
		const blockValue = { ...this.content, $settings: this.settings };
		return html`
			<uui-ref-node standalone href=${(this.config?.showContentEdit ? this._workspaceEditPath : undefined) ?? ''}>
				<div class="selection-background" aria-hidden="true">&emsp;</div>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${blockValue}></umb-ufm-render>
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

			/* HACK: Stretches a space character (&emsp;) to be full-width to make the RTE block appear text-selectable. [LK,NL] */
			.selection-background {
				position: absolute;
				pointer-events: none;
				font-size: 100vw;
				inset: 0;
				overflow: hidden;
				z-index: 0;
			}

			umb-icon,
			umb-ufm-render {
				z-index: 1;

				&::selection {
					color: var(--uui-color-default-contrast);
				}
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
