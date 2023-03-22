import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbCodeEditorElement } from '../../../shared/components/code-editor/code-editor.element';
import { UmbTemplateWorkspaceContext } from './template-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			umb-code-editor {
				--editor-height: calc(100vh - 300px);
			}

			uui-box {
				margin: 1em;
				--uui-box-default-padding: 0;
			}

			uui-input {
				width: 100%;
				margin: 1em;
			}
		`,
	];

	public load(entityKey: string) {
		this.#templateWorkspaceContext.load(entityKey);
	}

	public create(parentKey: string | null) {
		this.#isNew = true;
		this.#templateWorkspaceContext.createScaffold(parentKey);
	}

	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);
	#isNew = false;

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#templateWorkspaceContext.name, (name) => {
			this._name = name;
		});

		this.observe(this.#templateWorkspaceContext.content, (content) => {
			this._content = content;
		});
	}

	// TODO: temp code for testing create and save
	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#templateWorkspaceContext.setName(value);
	}

	//TODO - debounce that
	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#templateWorkspaceContext.setContent(value);
	}

	#insertCode(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;

		this._codeEditor?.insert(`My hovercraft is full of eels`);
	}

	render() {
		// TODO: add correct UI elements
		return html`<umb-workspace-layout alias="Umb.Workspace.Template">
			<uui-input slot="header" .value=${this._name} @input=${this.#onNameInput}></uui-input>
			<uui-box>
				<uui-button color="danger" look="primary" slot="header" @click=${this.#insertCode}
					>Insert "My hovercraft is full of eels"</uui-button
				>

				<umb-code-editor
					language="razor"
					id="content"
					.code=${this._content ?? ''}
					@input=${this.#onCodeEditorInput}></umb-code-editor>
			</uui-box>
		</umb-workspace-layout>`;
	}
}

export default UmbTemplateWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace': UmbTemplateWorkspaceElement;
	}
}
