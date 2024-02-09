import type { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-block-workspace-editor')
export class UmbBlockWorkspaceEditorElement extends UmbLitElement {
	//
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	@property({ type: String, attribute: false })
	workspaceAlias?: string;

	render() {
		return this.workspaceAlias
			? html` <umb-workspace-editor alias=${this.workspaceAlias} headline=${'BLOCK EDITOR'}> </umb-workspace-editor> `
			: '';
	}

	static styles = [
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
