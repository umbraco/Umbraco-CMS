import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { UmbWorkspaceEntityElement } from '../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbMediaWorkspaceContext } from './media-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-document-workspace')
export class UmbMediaWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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

	private _workspaceContext: UmbMediaWorkspaceContext = new UmbMediaWorkspaceContext(this);

	@state()
	_unique?: string;

	public load(entityKey: string) {
		this._workspaceContext.load(entityKey);
		this._unique = entityKey;
	}

	public create(parentKey: string | null) {
		this._workspaceContext.createScaffold(parentKey);
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Media">
			${this._unique
				? html`
						<umb-workspace-action-menu
							slot="action-menu"
							entity-type="media"
							unique="${this._unique}"></umb-workspace-action-menu>
				  `
				: nothing}
		</umb-workspace-content>`;
	}
}

export default UmbMediaWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace': UmbMediaWorkspaceElement;
	}
}
