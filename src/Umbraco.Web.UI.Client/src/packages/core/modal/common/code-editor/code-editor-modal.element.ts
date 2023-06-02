import { customElement, query } from 'lit/decorators.js';
import { css, html } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbCodeEditorElement as UmbCodeEditor } from '../../../components/code-editor';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbCodeEditorModalData, UmbCodeEditorModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbInputEvent } from '@umbraco-cms/backoffice/events';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-code-editor-modal')
export class UmbCodeEditorModalElement extends UmbModalBaseElement<UmbCodeEditorModalData, UmbCodeEditorModalResult> {
	@query('umb-code-editor')
	_codeEditor?: UmbCodeEditor;

	#handleConfirm() {
		this.modalHandler?.submit({ content: this.data?.content ?? '' });
	}

	#handleCancel() {
		this.modalHandler?.reject();
	}

	// TODO => debounce?
	#onCodeEditorInput(e: UmbInputEvent) {
		e.preventDefault();
		if (!this.data) {
			return;
		}

		this.data.content = this._codeEditor?.code ?? '';
	}

	render() {
		return html`
			<umb-body-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<div id="editor-box">
					<umb-code-editor
						language=${ifDefined(this.data?.language)}
						.code=${this.data?.content ?? ''}
						@input=${this.#onCodeEditorInput}></umb-code-editor>
				</div>
				<div slot="actions">
					<uui-button id="cancel" label="Cancel" @click="${this.#handleCancel}">Cancel</uui-button>
					<uui-button
						id="confirm"
						color="${this.data?.color || 'positive'}"
						look="primary"
						label="${this.data?.confirmLabel || 'Submit'}"
						@click=${this.#handleConfirm}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#editor-box {
				padding: var(--uui-box-default-padding, var(--uui-size-space-5, 18px));
				height:100%;
				display:flex;
			}

			umb-code-editor {
				width:100%;
			}
		`,
	];
}

export default UmbCodeEditorModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-editor-modal': UmbCodeEditorModalElement;
	}
}
