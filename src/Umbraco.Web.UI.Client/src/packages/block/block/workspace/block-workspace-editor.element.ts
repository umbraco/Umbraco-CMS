import { UMB_BLOCK_WORKSPACE_CONTEXT } from './index.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			if (context) {
				this.observe(
					context.name,
					(name) => {
						this._headline = this.localize.string(name);
					},
					'observeOwnerContentElementTypeName',
				);
			} else {
				this.removeUmbControllerByAlias('observeOwnerContentElementTypeName');
			}
		});
	}

	@state()
	private _headline: string = '';

	override render() {
		return html`<umb-workspace-editor headline=${this._headline}></umb-workspace-editor>`;
	}

	static override readonly styles = [
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
