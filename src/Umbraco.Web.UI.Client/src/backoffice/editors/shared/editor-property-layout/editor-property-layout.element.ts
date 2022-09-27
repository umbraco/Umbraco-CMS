import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-editor-property-layout')
export class UmbEditorPropertyLayoutElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 200px 600px;
				gap: 32px;
			}
		`,
	];

	@property({ type: String })
	public label = '';

	@property({ type: String })
	public description = '';

	render() {
		return html`
			<div>
				<uui-label>${this.label}</uui-label>
				<slot name="property-action-menu"></slot>
				<p>${this.description}</p>
			</div>
			<div>
				<slot name="editor"></slot>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-property-layout': UmbEditorPropertyLayoutElement;
	}
}
