import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalMultiUrlPickerData {
	title?: string;
	hideAnchor?: boolean;
	selection?: string;
}

import { UmbTreeElement } from '../../../../backoffice/shared/components/tree/tree.element';

@customElement('umb-modal-layout-multi-url-picker')
export class UmbModalLayoutMultiUrlPickerElement extends UmbModalLayoutElement<UmbModalMultiUrlPickerData> {
	static styles = [
		UUITextStyles,
		css`
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin-bottom: var(--uui-size-space-3);
			}

			uui-input,
			uui-toggle,
			uui-label {
				width: 100%;
			}

			uui-input,
			uui-label {
				margin-bottom: var(--uui-size-space-6);
			}

			.link-settings {
				display: grid;
				grid-template-columns: 1fr 1fr;
				gap: var(--uui-size-space-6);
			}

			#select-media {
				display: block;
			}
		`,
	];

	@state()
	_selection = '';

	@state()
	_hideAnchor = false;

	@state()
	_title = '';

	connectedCallback() {
		super.connectedCallback();

		this._title = this.data?.title ?? '';
		this._hideAnchor = this.data?.hideAnchor ?? false;
		this._selection = this.data?.selection ?? '';
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		//TODO: Should multiple property be implemented here or be passed down into umb-tree?
		this._selection = element.selection[element.selection.length - 1];
		console.log(this._selection);
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close();
	}

	render() {
		return html`
			<umb-workspace-layout headline="Select Link">
				<uui-box>
					<div class="link-settings">
						<span>
							<uui-label for="link-input">Link</uui-label>
							${this._renderLinkUrlInput()}
						</span>
						<span>
							<uui-label for="anchor-input">Anchor / querystring</uui-label>
							<uui-input id="anchor-input" placeholder="#value or ?key=value" label="#value or ?key=value"></uui-input>
						</span>
					</div>

					<uui-label for="link-title-input">Link Title</uui-label>
					<uui-input id="link-title-input" placeholder="Enter a title" label="link title"></uui-input>

					<uui-label>Target</uui-label>
					<uui-toggle id="#target-toggle">Open the link in a new tab</uui-toggle>

					<hr />

					${this._renderTrees()}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}

	private _renderLinkUrlInput() {
		if (this._selection) {
			return html`
				<uui-input id="link-input" placeholder="URL" label="URL" .value="${this._selection}" disabled></uui-input>
			`;
		} else {
			return html`<uui-input id="link-input" placeholder="URL" label="URL"></uui-input>`;
		}
	}

	private _renderTrees() {
		return html`<uui-label for="search-input">Link to page</uui-label>
			<uui-input id="search-input" placeholder="Type to search"></uui-input>
			<umb-tree
				alias="Umb.Tree.Documents"
				@selected=${this._handleSelectionChange}
				.selection=${[this._selection]}
				selectable></umb-tree>

			<hr />

			<uui-label>Link to media</uui-label>

			<umb-tree
				alias="Umb.Tree.Media"
				@selected=${this._handleSelectionChange}
				.selection=${[this._selection]}
				selectable></umb-tree>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-multi-url-picker': UmbModalLayoutMultiUrlPickerElement;
	}
}
