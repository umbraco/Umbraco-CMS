import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, query } from 'lit/decorators.js';
import { css, html } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbCodeEditorElement as UmbCodeEditor } from '../../components/code-editor';
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
			<umb-workspace-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<uui-box>
					<umb-code-editor
						language=${ifDefined(this.data?.language)}
						.code=${this.data?.content ?? ''}
						@input=${this.#onCodeEditorInput}></umb-code-editor>
				</uui-box>
				<div slot="actions">
					<uui-button id="cancel" label="Cancel" @click="${this.#handleCancel}">Cancel</uui-button>
					<uui-button
						id="confirm"
						color="${this.data?.color || 'positive'}"
						look="primary"
						label="${this.data?.confirmLabel || 'Submit'}"
						@click=${this.#handleConfirm}></uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
	
	static styles = [
		UUITextStyles,
		css`
			uui-box {
				flex: 1;
				--editor-height: calc(100vh - var(--umb-header-layout-height) - (var(--uui-size-space-5) * 2) - 54px);
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
