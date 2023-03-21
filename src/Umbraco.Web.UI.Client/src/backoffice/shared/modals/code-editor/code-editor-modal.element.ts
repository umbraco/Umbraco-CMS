import { UmbModalHandler } from '@umbraco-cms/backoffice/modal';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { html } from 'lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbCodeEditorModalData, UmbCodeEditorModalResult } from '.';

// TODO => Integrate with code editor
@customElement('umb-code-editor-modal')
export class UmbCodeEditorModalElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbCodeEditorModalData, UmbCodeEditorModalResult>;

	@property({ type: Object })
	data?: UmbCodeEditorModalData;

	#handleConfirm() {
		this.modalHandler?.submit({ content: this.data?.content });
	}

	#handleCancel() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-workspace-layout .headline=${this.data?.headline ?? 'Code Editor'}>
				<uui-box>
					<uui-code-block>${this.data?.content}</uui-code-block>
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
