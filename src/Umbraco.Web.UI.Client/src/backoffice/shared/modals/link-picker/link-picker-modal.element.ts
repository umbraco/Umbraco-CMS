import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { UUIBooleanInputEvent, UUIInputElement } from '@umbraco-ui/uui';
import { UmbTreeElement } from '../../components/tree/tree.element';
import {
	UmbLinkPickerConfig,
	UmbLinkPickerLink,
	UmbLinkPickerModalData,
	UmbLinkPickerModalResult,
} from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { buildUdi, getKeyFromUdi } from '@umbraco-cms/backoffice/utils';

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalResult> {
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
	_selectedKey?: string;

	@state()
	_index: number | null = null;

	@state()
	_link: UmbLinkPickerLink = {
		icon: null,
		name: null,
		published: true,
		queryString: null,
		target: null,
		trashed: false,
		udi: null,
		url: null,
	};

	@state()
	_layout: UmbLinkPickerConfig = {
		hideAnchor: false,
		ignoreUserStartNodes: false,
	};

	@query('#link-input')
	private _linkInput!: UUIInputElement;

	@query('#anchor-input')
	private _linkQueryInput?: UUIInputElement;

	@query('#link-title-input')
	private _linkTitleInput!: UUIInputElement;

	connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;
		this._index = this.data?.index;
		this._link = this.data?.link;
		this._layout = this.data?.config;

		if (!this._link.udi) return;
		this._selectedKey = getKeyFromUdi(this._link.udi);
	}

	private _handleQueryString() {
		if (!this._linkQueryInput) return;
		const query = this._linkQueryInput.value as string;
		//TODO: Handle query strings (add # etc)

		this._link.queryString = query;
	}

	private _handleSelectionChange(e: CustomEvent, entityType: string) {
		//TODO: Update icon, published, trashed
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		const selectedKey = element.selection[element.selection.length - 1];
		const udi = buildUdi(entityType, selectedKey);

		this._selectedKey = selectedKey;
		this._link.udi = udi;
		this._link.url = udi; // TODO
		this.requestUpdate();
	}

	private _submit() {
		this.modalHandler?.submit({ index: this._index, link: this._link });
	}

	private _close() {
		this.modalHandler?.reject();
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
						@input=${() => (this._link.name = this._linkTitleInput.value as string)}
						.value="${this._link.name ?? ''}"></uui-input>

					<uui-label>Target</uui-label>
					<uui-toggle
						id="#target-toggle"
						label="Toggle if link should open in a new tab"
						.checked="${this._link.target === '_blank' ? true : false}"
						@change="${(e: UUIBooleanInputEvent) =>
							e.target.checked ? (this._link.target = '_blank') : (this._link.target = '')}">
						Open the link in a new tab
					</uui-toggle>

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
				.value="${this._link.udi ?? this._link.url ?? ''}"
				@input=${() => (this._link.url = this._linkInput.value as string)}
				.disabled="${this._link.udi ? true : false}"></uui-input>
		</span>`;
	}

	private _renderAnchorInput() {
		if (this._layout.hideAnchor) return nothing;
		return html`<span>
			<uui-label for="anchor-input">Anchor / querystring</uui-label>
			<uui-input
				id="anchor-input"
				placeholder="#value or ?key=value"
				label="#value or ?key=value"
				@input=${this._handleQueryString}
				.value="${this._link.queryString ?? ''}"></uui-input>
		</span>`;
	}

	private _renderTrees() {
		return html`<uui-label for="search-input">Link to page</uui-label>
			<uui-input id="search-input" placeholder="Type to search" label="Type to search"></uui-input>
			<umb-tree
				alias="Umb.Tree.Documents"
				@selected=${(event: CustomEvent) => this._handleSelectionChange(event, 'document')}
				.selection=${[this._selectedKey ?? '']}
				selectable></umb-tree>

			<hr />

			<uui-label>Link to media</uui-label>

			<umb-tree
				alias="Umb.Tree.Media"
				@selected=${(event: CustomEvent) => this._handleSelectionChange(event, 'media')}
				.selection=${[this._selectedKey ?? '']}
				selectable></umb-tree>`;
	}
}

export default UmbLinkPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-modal': UmbLinkPickerModalElement;
	}
}
