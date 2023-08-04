import { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';
import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-workspace-editor')
export class UmbStylesheetWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: UmbStylesheetWorkspaceContext;

	@state()
	private _name?: string;

	@state()
	private _path?: string;

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbStylesheetWorkspaceContext;
			this.#observeNameAndPath();
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#observeNameAndPath() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name), '_observeName');
		this.observe(this.#workspaceContext.path, (path) => (this._path = path), '_observePath');
	}

	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.StyleSheet">
				<div id="header" slot="header">
					<uui-input label="stylesheet name" id="name" .value=${this._name} @input="${this.#onNameChange}"> </uui-input>
					<small>${this._path}</small>
				</div>

				<div slot="footer-info">
					<!-- TODO: Shortcuts Modal? -->
					<uui-button label="Show keyboard shortcuts">
						Keyboard Shortcuts
						<uui-keyboard-shortcut>
							<uui-key>ALT</uui-key>
							+
							<uui-key>shift</uui-key>
							+
							<uui-key>k</uui-key>
						</uui-keyboard-shortcut>
					</uui-button>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UUITextStyles,
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
