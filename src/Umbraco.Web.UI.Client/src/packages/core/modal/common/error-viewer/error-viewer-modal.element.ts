import { UmbModalBaseElement } from '../../component/modal-base.element.js';
import type { UmbErrorViewerModalData, UmbErrorViewerModalValue } from './error-viewer-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-error-viewer-modal')
export class UmbErrorViewerModalElement extends UmbModalBaseElement<UmbErrorViewerModalData, UmbErrorViewerModalValue> {
	@state()
	_displayError?: string;

	@state()
	_displayLang?: string;

	// Code adapted from https://stackoverflow.com/a/57668208/12787
	// Licensed under the permissions of the CC BY-SA 4.0 DEED
	#stringify(obj: any): string {
		let output = '{';
		for (const key in obj) {
			let value = obj[key];
			if (typeof value === 'function') {
				value = value.toString();
			} else if (value instanceof Array) {
				value = JSON.stringify(value);
			} else if (typeof value === 'object') {
				value = this.#stringify(value);
			} else {
				value = `"${value}"`;
			}
			output += `\n  ${key}: ${value},`;
		}
		return output + '\n}';
	}

	public override set data(value: UmbErrorViewerModalData | undefined) {
		super.data = value;
		// is JSON:
		if (typeof value === 'string') {
			this._displayLang = 'String';
			this._displayError = value;
		} else {
			this._displayLang = 'JSON';
			this._displayError = this.#stringify(value);
		}
	}
	public override get data(): UmbErrorViewerModalData | undefined {
		return super.data;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_seeErrorDialogHeadline')} main-no-padding>
				${this.data
					? html`<umb-code-block language=${this._displayLang ?? ''} copy>${this._displayError}</umb-code-block>`
					: nothing}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			umb-code-block {
				border: none;
				height: 100%;
			}
		`,
	];
}

export default UmbErrorViewerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-error-viewer-modal': UmbErrorViewerModalElement;
	}
}
