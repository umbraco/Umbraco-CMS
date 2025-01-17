import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from './stylesheet-workspace.context-token.js';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-stylesheet-workspace-editor')
export class UmbStylesheetWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean;

	@state()
	private _name?: string;

	#context?: typeof UMB_STYLESHEET_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context.name, (name) => (this._name = name));
			this.observe(this.#context.isNew, (isNew) => (this._isNew = isNew));
		});
	}

	#onNameInput(event: UUIInputEvent) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#context?.setName(value);
	}

	override render() {
		return html` <umb-entity-detail-workspace-editor> ${this.#renderHeader()} </umb-entity-detail-workspace-editor> `;
	}

	#renderHeader() {
		return html`
			<div id="workspace-header" slot="header">
				<uui-input
					placeholder=${this.localize.term('placeholders_entername')}
					.value=${this._name}
					@input=${this.#onNameInput}
					label=${this.localize.term('placeholders_entername')}
					?readonly=${this._isNew === false}
					${umbFocus()}>
				</uui-input>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			umb-code-editor {
				--editor-height: calc(100dvh - 260px);
			}

			uui-box {
				min-height: calc(100dvh - 260px);
				margin: var(--uui-size-layout-1);
				--uui-box-default-padding: 0;
				/* remove header border bottom as code editor looks better in this box */
				--uui-color-divider-standalone: transparent;
			}

			#workspace-header {
				width: 100%;
			}

			uui-input {
				width: 100%;
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
