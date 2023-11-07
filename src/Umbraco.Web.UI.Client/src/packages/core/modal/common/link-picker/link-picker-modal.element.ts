import { UmbTreeElement } from '../../../tree/tree.element.js';
import { css, html, nothing, customElement, query, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UUIBooleanInputEvent, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbLinkPickerConfig,
	UmbLinkPickerLink,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { buildUdi, getKeyFromUdi } from '@umbraco-cms/backoffice/utils';

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalValue> {
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

	@state()
	documentExpand = false;

	@state()
	mediaExpanded = false;

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

		if (query.startsWith('#') || query.startsWith('?')) {
			this._link.queryString = query;
			return;
		}

		if (query.includes('=')) {
			this._link.queryString = `?${query}`;
		} else {
			this._link.queryString = `#${query}`;
		}
	}

	private _handleSelectionChange(e: CustomEvent, entityType: string) {
		//TODO: Update icon, published, trashed
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		const selectedKey = element.selection[element.selection.length - 1];

		if (!selectedKey) {
			this._link.url = '';
			this._link.udi = undefined;
			this._selectedKey = undefined;
			this.requestUpdate();
			return;
		}

		const udi = buildUdi(entityType, selectedKey);

		this._selectedKey = selectedKey;
		this._link.udi = udi;
		this._link.url = udi;
		this.requestUpdate();
	}

	private _submit() {
		this.modalContext?.submit({ index: this._index, link: this._link });
	}

	private _close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Select Link">
				<uui-box>
					<div class="url-link">${this._renderLinkUrlInput()} ${this._renderAnchorInput()}</div>

					<uui-label for="link-title-input">${this.localize.term('defaultdialogs_nodeNameLinkPicker')}</uui-label>
					<uui-input
						id="link-title-input"
						placeholder=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}
						label=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}
						@input=${() => (this._link.name = this._linkTitleInput.value as string)}
						.value="${this._link.name ?? ''}"></uui-input>

					<uui-label>${this.localize.term('content_target')}</uui-label>
					<uui-toggle
						id="#target-toggle"
						label=${this.localize.term('defaultdialogs_openInNewWindow')}
						.checked="${this._link.target === '_blank' ? true : false}"
						@change="${(e: UUIBooleanInputEvent) =>
							e.target.checked ? (this._link.target = '_blank') : (this._link.target = '')}">
						${this.localize.term('defaultdialogs_openInNewWindow')}
					</uui-toggle>

					<hr />

					${this._renderTrees()}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._close}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	private _renderLinkUrlInput() {
		return html`<span>
			<uui-label for="link-input">${this.localize.term('defaultdialogs_link')}</uui-label>
			<uui-input
				id="link-input"
				placeholder=${this.localize.term('general_url')}
				label=${this.localize.term('general_url')}
				.value="${this._link.udi ?? this._link.url ?? ''}"
				@input=${() => (this._link.url = this._linkInput.value as string)}
				?disabled="${this._link.udi ? true : false}"></uui-input>
		</span>`;
	}

	private _renderAnchorInput() {
		if (this._layout.hideAnchor) return nothing;
		return html`<span>
			<uui-label for="anchor-input">${this.localize.term('defaultdialogs_anchorLinkPicker')}</uui-label>
			<uui-input
				id="anchor-input"
				placeholder=${this.localize.term('placeholders_anchor')}
				label=${this.localize.term('placeholders_anchor')}
				@input=${this._handleQueryString}
				.value="${this._link.queryString ?? ''}"></uui-input>
		</span>`;
	}

	private _renderTrees() {
		//TODO: Make search work
		return html`
			<uui-symbol-expand
				id="document-expand"
				@click=${() => (this.documentExpand = !this.documentExpand)}
				.open=${!this.documentExpand}></uui-symbol-expand>
			<uui-label for="document-expand">${this.localize.term('defaultdialogs_linkToPage')}</uui-label>
			<div style="${styleMap({ display: !this.documentExpand ? 'block' : 'none' })}">
				<uui-input
					id="search-input"
					placeholder=${this.localize.term('placeholders_search')}
					label=${this.localize.term('placeholders_search')}></uui-input>
				<umb-tree
					?hide-tree-root=${true}
					?multiple=${false}
					alias="Umb.Tree.Documents"
					@selection-change=${(event: CustomEvent) => this._handleSelectionChange(event, 'document')}
					.selection=${[this._selectedKey ?? '']}
					selectable></umb-tree>
			</div>
			<hr />
			<uui-symbol-expand
				id="media-expand"
				@click=${() => (this.mediaExpanded = !this.mediaExpanded)}
				.open=${!this.mediaExpanded}></uui-symbol-expand>
			<uui-label for="media-expand">${this.localize.term('defaultdialogs_linkToMedia')}</uui-label>
			<div style="${styleMap({ display: !this.mediaExpanded ? 'block' : 'none' })}">
				<umb-tree
					?hide-tree-root=${true}
					?multiple=${false}
					alias="Umb.Tree.Media"
					@selection-change=${(event: CustomEvent) => this._handleSelectionChange(event, 'media')}
					.selection=${[this._selectedKey ?? '']}
					selectable></umb-tree>
			</div>
		`;
	}

	static styles = [
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

			uui-input:not(#search-input),
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
}

export default UmbLinkPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-modal': UmbLinkPickerModalElement;
	}
}
