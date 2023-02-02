import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { UmbWorkspaceEntityElement } from '../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbDocumentWorkspaceContext } from './document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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

	private _workspaceContext: UmbDocumentWorkspaceContext = new UmbDocumentWorkspaceContext(this);

	public load(entityKey: string) {
		this._workspaceContext.load(entityKey);
	}

	public create(parentKey: string | null) {
		this._workspaceContext.createScaffold(parentKey);
	}

	render() {
		return html`<umb-workspace-content entity-type="document" alias="Umb.Workspace.Document"></umb-workspace-content>`;
	}
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
