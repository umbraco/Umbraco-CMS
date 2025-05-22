import type { UmbWorkspaceModalData } from './workspace-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-workspace-modal')
export class UmbWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	public get data(): UmbWorkspaceModalData | undefined {
		return this._data;
	}
	public set data(value: UmbWorkspaceModalData | undefined) {
		this._data = value;
		if (value?.inheritValidationLook) {
			// Do nothing.
		} else {
			const elementStyle = this.style;
			elementStyle.setProperty('--uui-color-invalid', 'var(--uui-color-danger)');
			elementStyle.setProperty('--uui-color-invalid-emphasis', 'var(--uui-color-danger-emphasis)');
			elementStyle.setProperty('--uui-color-invalid-standalone', 'var(--uui-color-danger-standalone)');
			elementStyle.setProperty('--uui-color-invalid-contrast', 'var(--uui-color-danger-contrast)');
		}
	}
	private _data?: UmbWorkspaceModalData | undefined;

	/**
	 * TODO: Consider if this binding and events integration is the right for communicating back the modal handler. Or if we should go with some Context API. like a Modal Context API.
	 *
	 */
	override render() {
		return this.data ? html`<umb-workspace .entityType=${this.data.entityType}></umb-workspace>` : '';
	}

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];
}

export default UmbWorkspaceModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-modal': UmbWorkspaceModalElement;
	}
}
