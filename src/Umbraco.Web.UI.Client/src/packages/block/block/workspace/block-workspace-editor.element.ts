import { UMB_BLOCK_WORKSPACE_CONTEXT } from './index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			if (context) {
				this.observe(
					observeMultiple([
						context.isNew,
						context.content.structure.ownerContentTypeObservablePart((contentType) => contentType?.name),
					]),
					([isNew, name]) => {
						this._headline =
							this.localize.term(isNew ? 'general_add' : 'general_edit') + ' ' + this.localize.string(name);
					},
					'observeOwnerContentElementTypeName',
				);
			} else {
				this.removeUmbControllerByAlias('observeOwnerContentElementTypeName');
			}
		});
	}

	@state()
	_headline: string = '';

	override render() {
		return html`<umb-workspace-editor headline=${this._headline}> </umb-workspace-editor> `;
	}

	static override readonly styles = [
		UmbTextStyles,
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
