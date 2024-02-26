import type { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-stylesheet-workspace-editor')
export class UmbStylesheetWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean = false;

	@state()
	private _path?: string;

	@state()
	private _name?: string;

	#workspaceContext?: UmbStylesheetWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbStylesheetWorkspaceContext;

			this.observe(this.#workspaceContext.path, (path) => (this._path = path), '_observeStylesheetPath');

			this.observe(this.#workspaceContext.name, (name) => (this._name = name), '_observeStylesheetName');

			this.observe(this.#workspaceContext.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#onNameChange(event: UUIInputEvent) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#workspaceContext?.setName(value);
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.StyleSheet">
				<div id="header" slot="header">
					<uui-input
						placeholder="Enter stylesheet name..."
						label="Stylesheet name"
						id="name"
						.value=${this._name}
						@input="${this.#onNameChange}"
						?readonly=${this._isNew === false}>
					</uui-input>
					<small>/css${this._path}</small>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
				flex-direction: column;
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}
		`,
	];
}

export default UmbStylesheetWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-editor': UmbStylesheetWorkspaceEditorElement;
	}
}
