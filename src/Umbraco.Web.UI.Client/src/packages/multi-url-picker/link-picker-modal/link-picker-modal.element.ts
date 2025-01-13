import type { UmbLinkPickerLink } from './types.js';
import type {
	UmbLinkPickerConfig,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
} from './link-picker-modal.token.js';
import { css, customElement, html, nothing, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { isUmbracoFolder, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';
import { UmbMediaDetailRepository } from '@umbraco-cms/backoffice/media';
import { umbConfirmModal, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UUIBooleanInputEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

type UmbInputPickerEvent = CustomEvent & { target: { value?: string } };

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalValue> {
	#linkTypePropertyLabels = {
		document: 'Selected document',
		media: 'Selected media',
	};

	#propertyLayoutOrientation: 'horizontal' | 'vertical' = 'vertical';

	@state()
	private _config: UmbLinkPickerConfig = {
		hideAnchor: false,
		hideTarget: false,
	};

	@state()
	private _allowedMediaTypeUniques?: Array<string>;

	@query('umb-input-document')
	private _documentPickerElement?: UmbInputDocumentElement;

	@query('umb-input-media')
	private _mediaPickerElement?: UmbInputMediaElement;

	override async firstUpdated() {
		if (this.data?.config) {
			this._config = this.data.config;
		}

		if (this.modalContext) {
			this.observe(this.modalContext.size, (size) => {
				if (size === 'large' || size === 'full') {
					this.#propertyLayoutOrientation = 'horizontal';
				}
			});
		}

		// Edge-cases: If the link is external and has an anchor/querystring, we will append it to the URL.
		if (this.value.link.type === 'external' && this.value.link.queryString) {
			this.#partialUpdateLink({ url: (this.value.link.url ?? '') + this.value.link.queryString, queryString: null });
		}

		// Get all the media types, excluding the folders, so that files are selectable media items.
		const mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);
		const { data: mediaTypes } = await mediaTypeStructureRepository.requestAllowedChildrenOf(null);
		this._allowedMediaTypeUniques =
			(mediaTypes?.items.map((x) => x.unique).filter((x) => x && !isUmbracoFolder(x)) as Array<string>) ?? [];
	}

	#partialUpdateLink(linkObject: Partial<UmbLinkPickerLink>) {
		this.modalContext?.updateValue({ link: { ...this.value.link, ...linkObject } });
	}

	#onLinkAnchorInput(event: UUIInputEvent) {
		const query = (event.target.value as string) ?? '';
		if (query.startsWith('#') || query.startsWith('?')) {
			this.#partialUpdateLink({ queryString: query });
			return;
		}

		if (query.includes('=')) {
			this.#partialUpdateLink({ queryString: `?${query}` });
		} else if (query) {
			this.#partialUpdateLink({ queryString: `#${query}` });
		} else {
			this.#partialUpdateLink({ queryString: query });
		}
	}

	#onLinkTitleInput(event: UUIInputEvent) {
		this.#partialUpdateLink({ name: event.target.value as string });
	}

	#onLinkTargetInput(event: UUIBooleanInputEvent) {
		this.#partialUpdateLink({ target: event.target.checked ? '_blank' : undefined });
	}

	#onLinkUrlInput(event: UUIInputEvent) {
		const url = event.target.value as string;

		let name;
		if (url && !this.value.link.name) {
			if (URL.canParse(url)) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				const parts = URL.parse(url);
				name = parts?.hostname ?? url;
			} else {
				name = url;
			}
		}

		this.#partialUpdateLink({
			name: this.value.link.name || name,
			type: 'external',
			url,
		});
	}

	async #onPickerSelection(event: UmbInputPickerEvent, type: 'document' | 'media') {
		let icon, name, url;
		const unique = event.target.value;

		if (unique) {
			if (type === 'document') {
				const documentRepository = new UmbDocumentDetailRepository(this);
				const { data: documentData } = await documentRepository.requestByUnique(unique);
				if (documentData) {
					icon = documentData.documentType.icon;
					name = documentData.variants[0].name;
					url = documentData.urls[0]?.url ?? '/';
				}
			}

			if (type === 'media') {
				const mediaRepository = new UmbMediaDetailRepository(this);
				const { data: mediaData } = await mediaRepository.requestByUnique(unique);
				if (mediaData) {
					icon = mediaData.mediaType.icon;
					name = mediaData.variants[0].name;
					url = mediaData.urls[0].url;
				}
			}
		}

		const link = {
			icon,
			name: this.value.link.name || name,
			type: unique ? type : undefined,
			unique,
			url: url ?? this.value.link.url,
		};

		this.#partialUpdateLink(link);
	}

	async #resetLink() {
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `Reset link?`,
			content: 'Are you sure you want to reset this link?',
			confirmLabel: 'Reset',
		});

		this.#partialUpdateLink({ name: null, queryString: null, target: null, type: null, unique: null, url: null });
	}

	#triggerDocumentPicker() {
		this._documentPickerElement?.shadowRoot?.querySelector('#btn-add')?.dispatchEvent(new Event('click'));
	}

	#triggerMediaPicker() {
		this._mediaPickerElement?.shadowRoot?.querySelector('#btn-add')?.dispatchEvent(new Event('click'));
	}

	#triggerExternalUrl() {
		const url = this.value.link.queryString
			? (this.value.link.url ?? '') + this.value.link.queryString
			: this.value.link.url;
		this.#partialUpdateLink({ type: 'external', url });
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectLink')}>
				<uui-box>
					${this.#renderLinkType()} ${this.#renderLinkTitleInput()} ${this.#renderLinkUrlInput()}
					${this.#renderLinkQuerystringInput()} ${this.#renderLinkTargetInput()}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						color="danger"
						look="secondary"
						label="Reset link"
						?disabled=${!this.value.link.type && !this.value.link.url}
						@click=${this.#resetLink}></uui-button>
					<uui-button
						color="positive"
						look="primary"
						label=${this.localize.term('general_submit')}
						?disabled=${!this.value.link.url && !this.value.link.queryString}
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderLinkType() {
		if (this.value.link.type === 'external') return nothing;

		const propertyLabel = this.value.link.type
			? this.#linkTypePropertyLabels[this.value.link.type]
			: 'Select link type';

		return html`
			<umb-property-layout orientation=${this.#propertyLayoutOrientation} label=${propertyLabel}>
				<div slot="editor">
					${this.#renderLinkTypeSelection()} ${this.#renderDocumentPicker()} ${this.#renderMediaPicker()}
				</div>
			</umb-property-layout>
		`;
	}

	#renderLinkTypeSelection() {
		if (this.value.link.type) return nothing;
		return html`
			<div id="select">
				<uui-button
					look="placeholder"
					label=${this.localize.term('defaultdialogs_linkToPage')}
					@click=${this.#triggerDocumentPicker}></uui-button>
				<uui-button
					look="placeholder"
					label=${this.localize.term('defaultdialogs_linkToMedia')}
					@click=${this.#triggerMediaPicker}></uui-button>
				<uui-button look="placeholder" label="Manually enter link" @click=${this.#triggerExternalUrl}></uui-button>
			</div>
		`;
	}

	#renderDocumentPicker() {
		return html`
			<umb-input-document
				?hidden=${!this.value.link.unique || this.value.link.type !== 'document'}
				.max=${1}
				.showOpenButton=${true}
				.value=${this.value.link.unique && this.value.link.type === 'document' ? this.value.link.unique : ''}
				@change=${(e: UmbInputPickerEvent) => this.#onPickerSelection(e, 'document')}>
			</umb-input-document>
		`;
	}

	#renderMediaPicker() {
		return html`
			<umb-input-media
				?hidden=${!this.value.link.unique || this.value.link.type !== 'media'}
				.allowedContentTypeIds=${this._allowedMediaTypeUniques}
				.max=${1}
				.value=${this.value.link.unique && this.value.link.type === 'media' ? this.value.link.unique : ''}
				@change=${(e: UmbInputPickerEvent) => this.#onPickerSelection(e, 'media')}></umb-input-media>
		`;
	}

	#renderLinkUrlInput() {
		if (!this.value.link.type) return nothing;
		return html`
			<umb-property-layout orientation=${this.#propertyLayoutOrientation} label=${this.localize.term('general_url')}>
				${when(
					!this.value.link.unique,
					() => html`
						<div slot="description">
							<small>You can include the anchor or querystring with the URL.</small>
						</div>
					`,
				)}
				<uui-input
					slot="editor"
					placeholder="Enter URL"
					label="Enter URL"
					.value=${this.value.link.url ?? ''}
					?disabled=${this.value.link.unique ? true : false}
					@change=${this.#onLinkUrlInput}>
				</uui-input>
			</umb-property-layout>
		`;
	}

	#renderLinkQuerystringInput() {
		if (this._config.hideAnchor) return nothing;
		if (!this.value.link.unique) return nothing;
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('defaultdialogs_anchorLinkPicker')}>
				<uui-input
					slot="editor"
					placeholder=${this.localize.term('placeholders_anchor')}
					label=${this.localize.term('placeholders_anchor')}
					@change=${this.#onLinkAnchorInput}
					.value=${this.value.link.queryString ?? ''}></uui-input>
			</umb-property-layout>
		`;
	}

	#renderLinkTitleInput() {
		if (!this.value.link.type) return nothing;
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}>
				<uui-input
					slot="editor"
					label=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}
					placeholder=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}
					.value=${this.value.link.name ?? ''}
					@change=${this.#onLinkTitleInput}>
				</uui-input>
			</umb-property-layout>
		`;
	}

	#renderLinkTargetInput() {
		if (this._config.hideTarget) return nothing;
		if (!this.value.link.type) return nothing;
		return html`
			<umb-property-layout orientation=${this.#propertyLayoutOrientation} label=${this.localize.term('content_target')}>
				<uui-toggle
					slot="editor"
					label=${this.localize.term('defaultdialogs_openInNewWindow')}
					.checked=${this.value.link.target === '_blank' ? true : false}
					@change=${this.#onLinkTargetInput}>
					${this.localize.term('defaultdialogs_openInNewWindow')}
				</uui-toggle>
			</umb-property-layout>
		`;
	}

	static override styles = [
		css`
			uui-box {
				--uui-box-default-padding: 0 var(--uui-size-space-5);
			}

			uui-input {
				width: 100%;
			}

			#select {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
				grid-template-rows: repeat(auto-fill, minmax(160px, 1fr));

				uui-button {
					min-height: 160px;
				}
			}
		`,
	];
}

export default UmbLinkPickerModalElement;

export { UmbLinkPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-modal': UmbLinkPickerModalElement;
	}
}
