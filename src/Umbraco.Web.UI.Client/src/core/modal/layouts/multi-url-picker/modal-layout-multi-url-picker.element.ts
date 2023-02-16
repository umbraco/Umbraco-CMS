import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { UUIBooleanInputEvent, UUIInputElement, UUIToggleElement } from '@umbraco-ui/uui';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalMultiUrlPickerData {
	UrlString?: string;
	anchorString?: string;
	linkTitle?: string;
	target?: boolean;
	hideAnchor?: boolean;
	treeItem?: string;
	ignoreUserStartNodes?: boolean;
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

			.url-link {
				display: flex;
				gap: var(--uui-size-space-6);
			}
			.url-link span {
				flex: 1 1 0px;
			}

			#select-media {
				display: block;
			}
		`,
	];

	@state()
	_url = {
		title: '',
		href: '',
		anchor: '',
		target: false,
		treeItem: '',
	};

	@state()
	_layout = {
		hideAnchor: false,
		ignoreUserStartNodes: false,
	};

	@query('#link-input')
	private _linkInput!: UUIInputElement;

	@query('#anchor-input')
	private _anchorInput?: UUIInputElement;

	@query('#link-title')
	private _linkTitleInput!: UUIInputElement;

	@query('#target-toggle')
	private _targetToggle!: UUIToggleElement;

	connectedCallback() {
		super.connectedCallback();
		this._url.href = this.data?.UrlString ?? '';
		this._url.anchor = this.data?.anchorString ?? '';
		this._url.title = this.data?.linkTitle ?? '';
		this._url.target = this.data?.target ?? false;
		this._url.treeItem = this.data?.treeItem ?? '';
		this._layout.hideAnchor = this.data?.hideAnchor ?? false;
		this._layout.ignoreUserStartNodes = this.data?.ignoreUserStartNodes ?? false;
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._url.treeItem = element.selection[element.selection.length - 1];
		this.requestUpdate();
	}

	private _submit() {
		this.modalHandler?.close({
			selection: this._url,
		});
	}

	private _close() {
		this.modalHandler?.close();
	}

	render() {
		return html`
			<umb-workspace-layout headline="Select Link">
				<uui-box>
					<div class="url-link">${this._renderLinkUrlInput()} ${this._renderAnchorInput()}</div>

					<uui-label for="link-title-input">Link Title</uui-label>
					<uui-input
						id="link-title-input"
						placeholder="Enter a title"
						label="link title"
						.value="${this._url.title}"></uui-input>

					<uui-label>Target</uui-label>
					<uui-toggle
						id="#target-toggle"
						.checked="${this._url.target}"
						@change="${(e: UUIBooleanInputEvent) => (this._url.target = e.target.checked)}"
						>Open the link in a new tab</uui-toggle
					>

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
		return html`<span>
			<uui-label for="link-input">Link</uui-label>
			<uui-input
				id="link-input"
				placeholder="URL"
				label="URL"
				.value="${this._url.treeItem}"
				.disabled="${this._url.treeItem ? true : false}"></uui-input>
		</span>`;
	}

	private _renderAnchorInput() {
		if (this._layout.hideAnchor) return;
		return html`<span>
			<uui-label for="anchor-input">Anchor / querystring</uui-label>
			<uui-input
				id="anchor-input"
				placeholder="#value or ?key=value"
				label="#value or ?key=value"
				.value="${this._url.anchor}"></uui-input>
		</span>`;
	}

	private _renderTrees() {
		return html`<uui-label for="search-input">Link to page</uui-label>
			<uui-input id="search-input" placeholder="Type to search"></uui-input>
			<umb-tree
				alias="Umb.Tree.Documents"
				@selected=${this._handleSelectionChange}
				.selection=${[this._url.treeItem]}
				selectable></umb-tree>

			<hr />

			<uui-label>Link to media</uui-label>

			<umb-tree
				alias="Umb.Tree.Media"
				@selected=${this._handleSelectionChange}
				.selection=${[this._url.treeItem]}
				selectable></umb-tree>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-multi-url-picker': UmbModalLayoutMultiUrlPickerElement;
	}
}
