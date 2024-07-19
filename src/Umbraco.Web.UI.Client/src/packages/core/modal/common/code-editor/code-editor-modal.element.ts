import { css, html, ifDefined, customElement, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UmbCodeEditorModalData, UmbCodeEditorModalValue } from '@umbraco-cms/backoffice/modal';

import '@umbraco-cms/backoffice/code-editor';

const elementName = 'umb-code-editor-modal';

@customElement(elementName)
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
	override render() {
		return html`
			<umb-body-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<div id="editor-box">${this.#renderCodeEditor()}</div>
				<div slot="actions">
					<uui-button
						id="cancel"
						label=${this.localize.term('general_cancel')}
						@click=${this.#handleCancel}></uui-button>
					<uui-button
						id="confirm"
						color="${this.data?.color || 'positive'}"
						look="primary"
						label="${this.data?.confirmLabel || this.localize.term('general_submit')}"
						@click=${this.#handleConfirm}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderCodeEditor() {
		return html`
			<umb-code-editor language=${ifDefined(this.data?.language)} .code=${this.data?.content ?? ''}></umb-code-editor>
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
		[elementName]: UmbCodeEditorModalElement;
	}
}
