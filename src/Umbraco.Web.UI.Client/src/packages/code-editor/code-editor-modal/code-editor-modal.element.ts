import type { UmbCodeEditorElement } from '../components/code-editor.element.js';
import type { UmbCodeEditorModalData, UmbCodeEditorModalValue } from './code-editor-modal.token.js';
import { css, customElement, html, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-code-editor-modal')
export class UmbCodeEditorModalElement extends UmbModalBaseElement<UmbCodeEditorModalData, UmbCodeEditorModalValue> {
	@query('umb-code-editor')
	_codeEditor?: UmbCodeEditorElement;

	#handleConfirm() {
		this.value = { content: this._codeEditor?.editor?.monacoEditor?.getValue() ?? '' };
		this.modalContext?.submit();
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	#onLoaded() {
		if (this.data?.formatOnLoad) {
			setTimeout(() => {
				this._codeEditor?.editor?.monacoEditor?.getAction('editor.action.formatDocument')?.run();
			}, 100);
		}
	}

	override render() {
		return html`
			<umb-body-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<div id="editor-box">${this.#renderCodeEditor()}</div>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this.#handleCancel}></uui-button>
				<uui-button
					slot="actions"
					color=${this.data?.color || 'positive'}
					look="primary"
					label=${this.data?.confirmLabel || this.localize.term('general_submit')}
					@click=${this.#handleConfirm}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderCodeEditor() {
		return html`
			<umb-code-editor
				language=${ifDefined(this.data?.language)}
				.code=${this.data?.content ?? ''}
				@loaded=${this.#onLoaded}></umb-code-editor>
		`;
	}

	static override styles = [
		css`
			#editor-box {
				padding: var(--uui-box-default-padding, var(--uui-size-space-5, 18px));
				height: 100%;
				display: flex;
			}

			umb-code-editor {
				width: 100%;
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
