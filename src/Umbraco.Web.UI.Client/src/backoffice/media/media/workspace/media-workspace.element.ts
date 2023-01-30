import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceMediaContext } from './media-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-media-workspace')
export class UmbMediaWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];


	private _workspaceContext: UmbWorkspaceMediaContext = new UmbWorkspaceMediaContext(this);

	public load(value: string) {
		this._workspaceContext?.load(value);
	}

	public create(parentKey: string | null) {
		this._workspaceContext?.create(parentKey);
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Media"></umb-workspace-content>`;
	}
}

export default UmbMediaWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace': UmbMediaWorkspaceElement;
	}
}
