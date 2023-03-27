import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { css, html } from 'lit';
import { UmbCodeEditorElement as CodeEditorElement } from '../../components/code-editor';
import { UmbCodeEditorModalData, UmbCodeEditorModalResult } from '.';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalHandler } from '@umbraco-cms/backoffice/modal';

// TODO => Integrate with code editor
@customElement('umb-code-editor-modal')
export class UmbCodeEditorModalElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`

		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbCodeEditorModalData, UmbCodeEditorModalResult>;

	@property({ type: Object })
	data?: UmbCodeEditorModalData;

	#handleConfirm() {
		console.log(this.data?.content)
		this.modalHandler?.submit({ content: this.data?.content ?? '' });
	}

	#handleCancel() {
		this.modalHandler?.reject();
	}

	// TODO => debounce?
	#onCodeEditorInput(event: Event) {
		if (!this.data) return;

		const target = event.target as CodeEditorElement;
		this.data.content = target.code as string;
	}

	render() {
		return html`
			<umb-workspace-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<uui-box>
					<umb-code-editor
						language=${this.data?.language ?? 'html'}
						id="content"
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
}

export default UmbCodeEditorModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-editor-modal': UmbCodeEditorModalElement;
	}
}
