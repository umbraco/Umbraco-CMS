import { UMB_DOCUMENT_LINK_PICKER_MODAL } from '../document-link-picker-modal/document-link-picker-modal.token.js';
import type { UmbLinkPickerLink } from './types.js';
import type {
	UmbLinkPickerConfig,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
} from './link-picker-modal.token.js';
import {
	createRef,
	css,
	customElement,
	html,
	nothing,
	query,
	ref,
	state,
	when,
	type Ref,
} from '@umbraco-cms/backoffice/external/lit';
import {
	umbBindToValidation,
	UmbObserveValidationStateController,
	UmbValidationContext,
} from '@umbraco-cms/backoffice/validation';
import { umbConfirmModal, UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentItemDataResolver,
	UmbDocumentItemRepository,
	UmbDocumentUrlRepository,
	UmbDocumentUrlsDataResolver,
	type UmbDocumentItemModel,
} from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UUIBooleanInputEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

type UmbInputPickerEvent = CustomEvent & { target: { value?: string; culture?: string } };

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalValue> {
	#propertyLayoutOrientation: 'horizontal' | 'vertical' = 'vertical';

	#validationContext = new UmbValidationContext(this);

	@state()
	private _config: UmbLinkPickerConfig = {
		hideAnchor: false,
		hideTarget: false,
	};

	@state()
	private _missingType = false;

	@query('umb-input-media')
	private _mediaPickerElement?: UmbInputMediaElement;

	@state()
	private _documentItem?: UmbDocumentItemModel;

	#variantContext = new UmbVariantContext(this).inherit();
	#documentItemDataResolver?: UmbDocumentItemDataResolver<UmbDocumentItemModel>;

	constructor() {
		super();

		new UmbObserveValidationStateController(this, '$.type', (invalid) => {
			this._missingType = invalid;
		});
	}

	override connectedCallback() {
		super.connectedCallback();

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

		this.populateLinkUrl();
	}

	override firstUpdated() {
		const type = this.value.link?.type;
		const unique = this.value.link?.unique;
		const culture = this.value.link?.culture;

		if (type === 'document' && unique) {
			this.#loadPickedDocumentItem(unique);
		}

		if (type === 'document' && culture) {
			this.#variantContext.setCulture(culture);
		}
	}

	async populateLinkUrl() {
		// Documents and media have URLs saved in the local link format. Display the actual URL to align with what
		// the user sees when they selected it initially.
		if (!this.value.link?.unique) return;

		let url: string | undefined = undefined;
		switch (this.value.link.type) {
			case 'document': {
				url = await this.#getUrlForDocument(this.value.link.unique);
				break;
			}
			case 'media': {
				url = await this.#getUrlForMedia(this.value.link.unique);
				break;
			}
			default:
				break;
		}

		if (url) {
			this.#partialUpdateLink({ url });
		}
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
			this.#partialUpdateLink({ queryString: '' });
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
		const unique = event.target.value;
		const culture = event.target.culture;
		this.#pickerSelect(type, unique, culture);
	}

	async #pickerSelect(type: 'document' | 'media', unique?: string, culture?: string) {
		let icon, name, url;

		if (unique) {
			switch (type) {
				case 'document': {
					await this.#loadPickedDocumentItem(unique);
					if (this._documentItem) {
						this.#documentItemDataResolver = new UmbDocumentItemDataResolver(this);
						this.#documentItemDataResolver.setData(this._documentItem);
						icon = await this.#documentItemDataResolver.getIcon();
						name = await this.#documentItemDataResolver.getName();
						url = await this.#getUrlForDocument(unique);
					}
					break;
				}
				case 'media': {
					const mediaRepository = new UmbMediaItemRepository(this);
					const { data: mediaData } = await mediaRepository.requestItems([unique]);
					const mediaItem = mediaData?.[0];
					if (mediaItem) {
						icon = mediaItem.mediaType.icon;
						name = mediaItem.variants[0].name;
						url = await this.#getUrlForMedia(unique);
					}
					break;
				}
				default:
					break;
			}
			// The selection was removed
		} else {
			this.#resetUrl();
		}

		const link = {
			icon,
			name: name || this.value.link.name,
			type: unique ? type : undefined,
			unique,
			url: url ?? this.value.link.url,
			culture: culture ?? this.value.link.culture,
		};

		this.#partialUpdateLink(link);
	}

	async #loadPickedDocumentItem(unique: string) {
		const documentRepository = new UmbDocumentItemRepository(this);
		const { data: documentItems } = await documentRepository.requestItems([unique]);
		this._documentItem = documentItems?.[0];
	}

	async #getUrlForDocument(unique: string) {
		const documentUrlRepository = new UmbDocumentUrlRepository(this);
		const { data: documentUrlData } = await documentUrlRepository.requestItems([unique]);
		const urlsItem = documentUrlData?.[0];

		const dataResolver = new UmbDocumentUrlsDataResolver(this);
		dataResolver.setData(urlsItem?.urls);
		const resolvedUrls = await dataResolver.getUrls();
		return resolvedUrls?.[0]?.url ?? '';
	}

	async #getUrlForMedia(unique: string) {
		const mediaUrlRepository = new UmbMediaUrlRepository(this);
		const { data: mediaUrlData } = await mediaUrlRepository.requestItems([unique]);
		return mediaUrlData?.[0].url ?? '';
	}

	async #onResetUrl() {
		if (this.value.link.url) {
			await umbConfirmModal(this, {
				color: 'danger',
				headline: this.localize.term('linkPicker_resetUrlHeadline'),
				content: this.localize.term('linkPicker_resetUrlMessage'),
				confirmLabel: this.localize.term('linkPicker_resetUrlLabel'),
			});
		}

		this.#resetUrl();
	}

	#resetUrl() {
		this.#partialUpdateLink({ type: null, url: null });
	}

	async #onSubmit() {
		try {
			await this.#validationContext.validate();
			this.modalContext?.submit();
		} catch {
			console.log('Validation failed');
		}
	}

	async #triggerDocumentLinkPicker() {
		const value = await umbOpenModal(this, UMB_DOCUMENT_LINK_PICKER_MODAL, {});

		// If a culture is selected for the document, we need to set the variant context to make sure we get the correct name and url for the document.
		// This will make a local variant context for the link picker when a Document with a culture is selected,
		// and will inherit the values from the parent context (if any), but will override the culture with the one selected for the document.
		if (value.culture) {
			this.#variantContext.setCulture(value.culture);
		}

		this.#pickerSelect('document', value.unique, value.culture);
	}

	#triggerMediaPicker() {
		this._mediaPickerElement?.shadowRoot?.querySelector('#btn-add')?.dispatchEvent(new Event('click'));
	}

	#triggerExternalUrl() {
		this.#partialUpdateLink({ type: 'external' });
	}

	#checkIfUrlIsMissing() {
		if (this.value.link.type !== 'external') return false;
		const hasUrl = this.value.link.url && this.value.link.url.length > 0;
		const hasAnchor = this.value.link.queryString && this.value.link.queryString.length > 0;

		return !hasUrl && !hasAnchor;
	}

	override render() {
		return html`
			<umb-body-layout
				headline=${this.localize.term(
					this.modalContext?.data?.isNew ? 'defaultdialogs_addLink' : 'defaultdialogs_updateLink',
				)}>
				<uui-box>
					${this.#renderLinkType()} ${this.#renderLinkAnchorInput()} ${this.#renderLinkTitleInput()}
					${this.#renderLinkTargetInput()}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						color="positive"
						look="primary"
						?disabled=${!this.value.link.type}
						label=${this.localize.term(this.modalContext?.data?.isNew ? 'general_add' : 'general_update')}
						@click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderLinkType() {
		return html`${this.#renderLinkTypeSelection()} ${this.#renderDocumentPicker()} ${this.#renderMediaPicker()}
		${this.#renderLinkUrlInput()} ${this.#renderEntryUrl()}`;
	}

	#renderLinkTypeSelection() {
		if (this.value.link.type) return nothing;
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('linkPicker_modalSource')}
				?invalid=${this._missingType}>
				<uui-button-group slot="editor">
					<uui-button
						data-mark="action:document"
						look="placeholder"
						label=${this.localize.term('general_content')}
						.color=${this._missingType ? 'invalid' : 'default'}
						@click=${this.#triggerDocumentLinkPicker}></uui-button>
					<uui-button
						data-mark="action:media"
						look="placeholder"
						label=${this.localize.term('general_media')}
						.color=${this._missingType ? 'invalid' : 'default'}
						@click=${this.#triggerMediaPicker}></uui-button>
					<uui-button
						data-mark="action:external"
						look="placeholder"
						label=${this.localize.term('linkPicker_modalManual')}
						.color=${this._missingType ? 'invalid' : 'default'}
						@click=${this.#triggerExternalUrl}></uui-button>
				</uui-button-group>
			</umb-property-layout>
		`;
	}

	#entityItemRef: Ref<HTMLInputElement> = createRef();

	#renderDocumentPicker() {
		return html`
			<umb-property-layout
				?hidden=${!this.value.link.unique || this.value.link.type !== 'document' || !this._documentItem}
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('general_content')}>
				<umb-entity-item-ref slot="editor" .item=${this._documentItem} ${ref(this.#entityItemRef)} standalone>
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_remove')}
							@click=${() => this.#onDocumentRemove()}></uui-button>
					</uui-action-bar>
				</umb-entity-item-ref>
			</umb-property-layout>
		`;
	}

	async #onDocumentRemove() {
		const name = await this.#documentItemDataResolver?.getName();

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove?`,
			content: `#defaultdialogs_confirmremove ${name}?`,
			confirmLabel: '#actions_remove',
		});

		this.#pickerSelect('document', undefined, undefined);
		this.#documentItemDataResolver?.destroy();
		this.#documentItemDataResolver = undefined;
		this._documentItem = undefined;
	}

	#renderMediaPicker() {
		return html`
			<umb-property-layout
				?hidden=${!this.value.link.unique || this.value.link.type !== 'media'}
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('general_media')}>
				<umb-input-media
					slot="editor"
					.max=${1}
					.value=${this.value.link.unique && this.value.link.type === 'media' ? this.value.link.unique : ''}
					@change=${(e: UmbInputPickerEvent) => this.#onPickerSelection(e, 'media')}></umb-input-media>
			</umb-property-layout>
		`;
	}

	#renderLinkUrlInput() {
		if (this.value.link.type !== 'external') return nothing;
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('linkPicker_modalManual')}>
				<uui-input
					slot="editor"
					data-mark="input:url"
					label=${this.localize.term('placeholders_enterUrl')}
					placeholder=${this.localize.term('placeholders_enterUrl')}
					.value=${this.value.link.url ?? ''}
					?disabled=${!!this.value.link.unique}
					@input=${this.#onLinkUrlInput}
					.error=${this.#checkIfUrlIsMissing()}
					.errorMessage=${this.localize.term('linkPicker_modalUrlOrAnchorValidationMessage')}
					${umbBindToValidation(this, '$.link.unique')}
					${umbFocus()}>
					${when(
						!this.value.link.unique,
						() => html`
							<div slot="append">
								<uui-button
									slot="append"
									label=${this.localize.term('general_remove')}
									@click=${this.#onResetUrl}></uui-button>
							</div>
						`,
					)}
				</uui-input>
			</umb-property-layout>
		`;
	}

	#renderEntryUrl() {
		if (!this.value.link.unique || !this.value.link.url) return nothing;
		return html` <uui-input readonly value=${this.value.link.url}></uui-input> `;
	}

	#renderLinkAnchorInput() {
		if (this._config.hideAnchor) return nothing;
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('defaultdialogs_anchorLinkPicker')}>
				<uui-input
					data-mark="input:anchor"
					slot="editor"
					label=${this.localize.term('placeholders_anchor')}
					placeholder=${this.localize.term('placeholders_anchor')}
					.error=${this.#checkIfUrlIsMissing()}
					.errorMessage=${this.localize.term('linkPicker_modalUrlOrAnchorValidationMessage')}
					.value=${this.value.link.queryString ?? ''}
					@input=${this.#onLinkAnchorInput}
					${umbBindToValidation(this, '$.link.queryString')}></uui-input>
			</umb-property-layout>
		`;
	}

	#renderLinkTitleInput() {
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('defaultdialogs_nodeNameLinkPicker')}>
				<uui-input
					data-mark="input:title"
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
			*[hidden] {
				display: none;
			}

			uui-box {
				--uui-box-default-padding: 0 var(--uui-size-space-5);
			}

			uui-button-group {
				width: 100%;
			}

			uui-input {
				width: 100%;

				&[readonly] {
					margin-top: var(--uui-size-space-2);
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
