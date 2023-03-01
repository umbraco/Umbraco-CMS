import { UUITextStyles } from '@umbraco-ui/uui-css';
import { html, TemplateResult } from 'lit-html';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalCodeEditorData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}
// TODO => This is not a code editor, it is a static display only. Needs to use ACE or similar
@customElement('umb-modal-layout-code-editor')
export class UmbModalLayoutCodeEditorElement extends UmbModalLayoutElement<UmbModalCodeEditorData> {
	static styles = [UUITextStyles];

	private _handleConfirm() {
		this.modalHandler?.close({ confirmed: true, content: this.data?.content });
	}

	private _handleCancel() {
		this.modalHandler?.close({ confirmed: false, content: null });
	}

	render() {
		return html`
			<umb-workspace-layout .headline=${this.data?.headline ?? "Edit Source Code"}>
				<uui-code-block>${this.data?.content}</uui-code-block>
				<div slot="actions">
					<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
					<uui-button
						slot="actions"
						id="confirm"
						color="${this.data?.color || 'positive'}"
						look="primary"
						label="${this.data?.confirmLabel || 'Submit'}"
						@click=${this._handleConfirm}></uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-code-editor': UmbModalLayoutCodeEditorElement;
	}
}
