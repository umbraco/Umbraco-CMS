import { UMB_BLOCK_WORKSPACE_CONTEXT } from './index.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.name,
				(name) => {
					if (name) {
						this._headline = this.localize.string(name);
					}
				},
				'observeOwnerContentElementTypeName',
			);
			this.observe(
				context?.readOnlyGuard.isPermittedForObservableVariant(context.variantId),
				(isReadOnly) => {
					this._readOnly = isReadOnly;
				},
				'observeIsReadOnly',
			);
		});
	}

	@state()
	private _headline: string = '';

	@state()
	private _readOnly?: boolean;

	override render() {
		return html`<umb-workspace-editor
			><div slot="header">
				<h3 id="headline" title="${this._headline}" data-mark="layout-headline">${this._headline}</h3>
				${this._readOnly
					? html`<uui-tag look="secondary">${this.localize.term('general_readOnly')}</uui-tag>`
					: nothing}
			</div></umb-workspace-editor
		>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
			div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-3);
			}
			#headline {
				display: block;
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
				min-width: 0;
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
