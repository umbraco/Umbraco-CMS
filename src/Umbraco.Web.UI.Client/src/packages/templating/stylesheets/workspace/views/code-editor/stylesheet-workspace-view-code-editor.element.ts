import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-workspace-view-code-editor')
export class UmbStylesheetWorkspaceViewCodeEditorElement extends UmbLitElement {
	render() {
		return html`umb-stylesheet-workspace-view-code-editor`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbStylesheetWorkspaceViewCodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-view-code-editor': UmbStylesheetWorkspaceViewCodeEditorElement;
	}
}
