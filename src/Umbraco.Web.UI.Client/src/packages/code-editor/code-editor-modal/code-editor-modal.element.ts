import type { UmbCodeEditorElement } from '../components/code-editor.element.js';
import type { UmbCodeEditorModalData, UmbCodeEditorModalValue } from './code-editor-modal.token.js';
import { css, customElement, html, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-code-editor-modal')
export class UmbCodeEditorModalElement extends UmbModalBaseElement<UmbCodeEditorModalData, UmbCodeEditorModalValue> {
	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#resizeObserver?: ResizeObserver;

	override disconnectedCallback(): void {
		this.#resizeObserver?.disconnect();
		this.#resizeObserver = undefined;
		super.disconnectedCallback();
	}

	#handleConfirm() {
		this.value = { content: this._codeEditor?.editor?.monacoEditor?.getValue() ?? '' };
		this.modalContext?.submit();
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	#onLoaded() {
		const monacoEditor = this._codeEditor?.editor?.monacoEditor;
		const layout = () => monacoEditor?.layout();

		layout();
		requestAnimationFrame(() => {
			layout();
			requestAnimationFrame(layout);
		});
		setTimeout(layout, 50);
		setTimeout(layout, 280);

		const box = this.shadowRoot?.getElementById('editor-box');
		this.#resizeObserver?.disconnect();
		if (box && typeof ResizeObserver !== 'undefined') {
			this.#resizeObserver = new ResizeObserver(() => layout());
			this.#resizeObserver.observe(box);
		}

		if (this.data?.formatOnLoad) {
			setTimeout(() => {
				void monacoEditor?.getAction('editor.action.formatDocument')?.run();
				setTimeout(layout, 120);
				setTimeout(layout, 250);
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
					label=${this.localize.string(this.data?.confirmLabel) || this.localize.term('general_submit')}
					@click=${this.#handleConfirm}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderCodeEditor() {
		const wordWrap = this.data?.language === 'html';
		return html`
			<umb-code-editor
				language=${ifDefined(this.data?.language)}
				.code=${this.data?.content ?? ''}
				minimap-size="fit"
				?word-wrap=${wordWrap}
				@loaded=${this.#onLoaded}></umb-code-editor>
		`;
	}

	static override styles = [
		css`
			#editor-box {
				box-sizing: border-box;
				padding: var(--uui-box-default-padding, var(--uui-size-space-5, 18px));
				height: 100%;
				min-height: 0;
				min-width: 0;
				display: flex;
				flex-direction: column;
				overflow: hidden;
				scrollbar-gutter: stable;
			}

			umb-code-editor {
				display: block;
				flex: 1 1 auto;
				width: 100%;
				min-width: 0;
				max-width: 100%;
				overflow: hidden;
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
