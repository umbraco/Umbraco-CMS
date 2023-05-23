import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbCodeEditorElement } from '../../../core/components/code-editor';
import { UmbPartialViewsWorkspaceContext } from './partial-views-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-partial-views-workspace-edit')
export class UmbPartialViewsWorkspaceEditElement extends UmbLitElement {
	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#partialViewsWorkspaceContext?: UmbPartialViewsWorkspaceContext;
	#isNew = false;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (workspaceContext: UmbPartialViewsWorkspaceContext) => {
			this.#partialViewsWorkspaceContext = workspaceContext;
			this.observe(this.#partialViewsWorkspaceContext.name, (name) => {
				this._name = name;
			});

			this.observe(this.#partialViewsWorkspaceContext.content, (content) => {
				this._content = content;
			});

			// this.observe(this.#partialViewsWorkspaceContext.isNew, (isNew) => {
			// 	this.#isNew = !!isNew;
			// });
		});
	}

	// TODO: temp code for testing create and save
	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#partialViewsWorkspaceContext?.setName(value);
	}

	//TODO - debounce that
	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#partialViewsWorkspaceContext?.setContent(value);
	}

	#insertCode(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;

		this._codeEditor?.insert(`My hovercraft is full of eels`);
	}

	render() {
		// TODO: add correct UI elements
		return html`<umb-body-layout alias="Umb.Workspace.Template">
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
		</umb-body-layout>`;
	}

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
}

export default UmbPartialViewsWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-views-workspace-edit': UmbPartialViewsWorkspaceEditElement;
	}
}
