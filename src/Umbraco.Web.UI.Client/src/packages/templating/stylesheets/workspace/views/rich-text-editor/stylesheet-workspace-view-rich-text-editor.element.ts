import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-workspace-view-rich-text-editor')
export class UmbStylesheetWorkspaceViewRichTextEditorElement extends UmbLitElement {
	render() {
		return html`umb-stylesheet-workspace-view-RICH_TEXT-editor`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbStylesheetWorkspaceViewRichTextEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-view-rich-text-editor': UmbStylesheetWorkspaceViewRichTextEditorElement;
	}
}
