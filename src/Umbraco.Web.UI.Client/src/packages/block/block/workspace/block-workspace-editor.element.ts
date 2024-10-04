import { UMB_BLOCK_WORKSPACE_CONTEXT } from './index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	//
	@property({ type: String, attribute: false })
	workspaceAlias?: string;

	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				observeMultiple([
					context.isNew,
					context.content.structure.ownerContentTypeObservablePart((contentType) => contentType?.name),
				]),
				([isNew, name]) => {
					this._headline = this.localize.term(isNew ? 'general_add' : 'general_edit') + ' ' + name;
				},
				'observeOwnerContentElementTypeName',
			);
		});
	}

	@state()
	_headline: string = '';

	override render() {
		return this.workspaceAlias
			? html` <umb-workspace-editor alias=${this.workspaceAlias} headline=${this._headline}> </umb-workspace-editor> `
			: nothing;
	}

	static override styles = [
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
