import type { UmbTreeElement, UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';
import { css, html, nothing, customElement, query, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import type { UUIBooleanInputEvent, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbLinkPickerConfig,
	UmbLinkPickerLink,
	UmbLinkPickerLinkType,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
} from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_TREE_ALIAS } from '@umbraco-cms/backoffice/document';

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalValue> {
	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	_selectedKey?: string;

	/**
	 * The link object, notice this is frozen, as it comes directly from the State. So it cannot be manipulated.
	 */
	@state()
	readonly _link: UmbLinkPickerLink = {};

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

		if (this.modalContext) {
			this.observe(this.modalContext.value, (value) => {
				(this._link as any) = value.link;
				this._selectedKey = this._link?.unique ?? undefined;
				this._selectionConfiguration.selection = this._selectedKey ? [this._selectedKey] : [];
			});
		}
		this._layout = this.data?.config;
	}

	#handleQueryString() {
		if (!this._linkQueryInput) return;
		const query = this._linkQueryInput.value as string;

		if (query.startsWith('#') || query.startsWith('?')) {
			this.#partialUpdateLink({ queryString: query });
			return;
		}

		if (query.includes('=')) {
			this.#partialUpdateLink({ queryString: `#${query}` });
		} else {
			this.#partialUpdateLink({ queryString: `#${query}` });
		}
	}

	#handleSelectionChange(e: CustomEvent, entityType: string) {
		//TODO: Update icon, published, trashed
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		const selection = element.getSelection();
		const selectedKey = selection[selection.length - 1];

		if (!selectedKey) {
			this.#partialUpdateLink({ type: undefined, unique: '', url: undefined });
			this._selectedKey = undefined;
			this._selectionConfiguration.selection = [];
			this.requestUpdate();
			return;
		}

		const linkType = (entityType as UmbLinkPickerLinkType) ?? 'external';

		this._selectedKey = selectedKey;
		this._selectionConfiguration.selection = [this._selectedKey];
		this.#partialUpdateLink({ type: linkType, unique: selectedKey, url: selectedKey });
		this.requestUpdate();
	}

	#partialUpdateLink(linkObject: Partial<UmbLinkPickerLink>) {
		this.modalContext?.updateValue({ link: { ...this._link, ...linkObject } });
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
						@input=${() => this.#partialUpdateLink({ name: this._linkTitleInput.value as string })}
						.value="${this._link.name ?? ''}"></uui-input>

					<uui-label>${this.localize.term('content_target')}</uui-label>
					<uui-toggle
						id="#target-toggle"
						label=${this.localize.term('defaultdialogs_openInNewWindow')}
						.checked="${this._link.target === '_blank' ? true : false}"
						@change="${(e: UUIBooleanInputEvent) =>
							this.#partialUpdateLink({ target: e.target.checked ? '_blank' : '' })}">
						${this.localize.term('defaultdialogs_openInNewWindow')}
					</uui-toggle>

					<hr />

					${this._renderTrees()}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
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
				.value="${this._link.unique ?? this._link.url ?? ''}"
				@input=${() => this.#partialUpdateLink({ type: 'external', url: this._linkInput.value as string })}
				?disabled="${this._link.unique ? true : false}"></uui-input>
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
				@input=${this.#handleQueryString}
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
					alias=${UMB_DOCUMENT_TREE_ALIAS}
					@selection-change=${(event: CustomEvent) => this.#handleSelectionChange(event, 'document')}
					.selectionConfiguration=${this._selectionConfiguration}></umb-tree>
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
					alias="Umb.Tree.Media"
					@selection-change=${(event: CustomEvent) => this.#handleSelectionChange(event, 'media')}
					.selectionConfiguration=${this._selectionConfiguration}></umb-tree>
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
