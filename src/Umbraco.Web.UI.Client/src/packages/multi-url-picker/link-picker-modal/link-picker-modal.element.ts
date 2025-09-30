import type { UmbLinkPickerLink } from './types.js';
import type {
	UmbLinkPickerConfig,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
} from './link-picker-modal.token.js';
import { css, customElement, html, nothing, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { isUmbracoFolder, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import {
	UMB_VALIDATION_CONTEXT,
	umbBindToValidation,
	UmbObserveValidationStateController,
	UmbValidationContext,
	type UmbValidator,
} from '@umbraco-cms/backoffice/validation';
import { umbConfirmModal, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentItemDataResolver,
	UmbDocumentItemRepository,
	UmbDocumentUrlRepository,
	UmbDocumentUrlsDataResolver,
} from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UUIBooleanInputEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

class UmbLinkPickerValueValidator extends UmbControllerBase implements UmbValidator {
	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;

	#isValid = true;
	get isValid(): boolean {
		return this.#isValid;
	}

	#value: unknown;

	#unique = 'UmbLinkPickerValueValidator';

	setValue(value: unknown) {
		this.#value = value;
		this.validate();
	}

	getValue(): unknown {
		return this.#value;
	}

	// The path to the data that this validator is validating.
	readonly #dataPath: string;

	constructor(host: UmbControllerHost, dataPath: string) {
		super(host);
		this.#dataPath = dataPath;
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#context) {
				this.#context.removeValidator(this);
			}
			this.#context = context;
			context?.addValidator(this);
		});
	}

	async validate(): Promise<void> {
		this.#isValid = !!this.getValue();

		if (this.#isValid) {
			this.#context?.messages.removeMessageByKey(this.#unique);
		} else {
			this.#context?.messages.addMessage(
				'client',
				this.#dataPath,
				'#linkPicker_modalAnchorValidationMessage',
				this.#unique,
			);
		}
	}

	reset(): void {}

	focusFirstInvalidElement(): void {}
}

type UmbInputPickerEvent = CustomEvent & { target: { value?: string } };

@customElement('umb-link-picker-modal')
export class UmbLinkPickerModalElement extends UmbModalBaseElement<UmbLinkPickerModalData, UmbLinkPickerModalValue> {
	#propertyLayoutOrientation: 'horizontal' | 'vertical' = 'vertical';

	#validationContext = new UmbValidationContext(this);

	@state()
	private _allowedMediaTypeUniques?: Array<string>;

	@state()
	private _config: UmbLinkPickerConfig = {
		hideAnchor: false,
		hideTarget: false,
	};

	@state()
	private _missingLinkUrl = false;

	@query('umb-input-document')
	private _documentPickerElement?: UmbInputDocumentElement;

	@query('umb-input-media')
	private _mediaPickerElement?: UmbInputMediaElement;

	constructor() {
		super();

		new UmbObserveValidationStateController(this, '$.type', (invalid) => {
			this._missingLinkUrl = invalid;
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

		this.#getMediaTypes();
		this.populateLinkUrl();

		const validator = new UmbLinkPickerValueValidator(this, '$.type');

		this.observe(this.modalContext?.value, (value) => {
			validator.setValue(value?.link.type);
		});
	}

	async #getMediaTypes() {
		// Get all the media types, excluding the folders, so that files are selectable media items.
		const mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);
		const { data: mediaTypes } = await mediaTypeStructureRepository.requestAllowedChildrenOf(null, null);
		this._allowedMediaTypeUniques =
			(mediaTypes?.items.map((x) => x.unique).filter((x) => x && !isUmbracoFolder(x)) as Array<string>) ?? [];
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
		let icon, name, url;
		const unique = event.target.value;

		if (unique) {
			switch (type) {
				case 'document': {
					const documentRepository = new UmbDocumentItemRepository(this);
					const { data: documentItems } = await documentRepository.requestItems([unique]);
					const documentItem = documentItems?.[0];
					if (documentItem) {
						const itemDataResolver = new UmbDocumentItemDataResolver(this);
						itemDataResolver.setData(documentItem);
						icon = await itemDataResolver.getIcon();
						name = await itemDataResolver.getName();
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
		};

		this.#partialUpdateLink(link);

		await this.#validationContext.validate();
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
		await this.#validationContext.validate();
		this.modalContext?.submit();
	}

	#triggerDocumentPicker() {
		this._documentPickerElement?.shadowRoot?.querySelector('#btn-add')?.dispatchEvent(new Event('click'));
	}

	#triggerMediaPicker() {
		this._mediaPickerElement?.shadowRoot?.querySelector('#btn-add')?.dispatchEvent(new Event('click'));
	}

	#triggerExternalUrl() {
		this.#partialUpdateLink({ type: 'external' });
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
						label=${this.localize.term(this.modalContext?.data?.isNew ? 'general_add' : 'general_update')}
						@click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderLinkType() {
		return html`
			<umb-property-layout
				orientation=${this.#propertyLayoutOrientation}
				label=${this.localize.term('linkPicker_modalSource')}
				mandatory
				?invalid=${this._missingLinkUrl}>
				<div slot="editor">
					${this.#renderLinkTypeSelection()} ${this.#renderDocumentPicker()} ${this.#renderMediaPicker()}
					${this.#renderLinkUrlInput()} ${this.#renderLinkUrlInputReadOnly()}
				</div>
			</umb-property-layout>
		`;
	}

	#renderLinkTypeSelection() {
		if (this.value.link.type) return nothing;
		return html`
			<uui-button-group>
				<uui-button
					data-mark="action:document"
					look="placeholder"
					label=${this.localize.term('general_content')}
					@click=${this.#triggerDocumentPicker}></uui-button>
				<uui-button
					data-mark="action:media"
					look="placeholder"
					label=${this.localize.term('general_media')}
					@click=${this.#triggerMediaPicker}></uui-button>
				<uui-button
					data-mark="action:external"
					look="placeholder"
					label=${this.localize.term('linkPicker_modalManual')}
					@click=${this.#triggerExternalUrl}></uui-button>
			</uui-button-group>
		`;
	}

	#renderDocumentPicker() {
		return html`
			<umb-input-document
				?hidden=${!this.value.link.unique || this.value.link.type !== 'document'}
				.max=${1}
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
		if (this.value.link.type !== 'external') return nothing;
		return html`
			<uui-input
				data-mark="input:url"
				label=${this.localize.term('placeholders_enterUrl')}
				placeholder=${this.localize.term('placeholders_enterUrl')}
				.value=${this.value.link.url ?? ''}
				?disabled=${!!this.value.link.unique}
				required
				@input=${this.#onLinkUrlInput}
				${umbBindToValidation(this)}
				${umbFocus()}>
				${when(
					!this.value.link.unique,
					() => html`
						<div slot="append">
							<uui-button
								slot="append"
								compact
								label=${this.localize.term('general_remove')}
								@click=${this.#onResetUrl}>
								<uui-icon name="remove"></uui-icon>
							</uui-button>
						</div>
					`,
				)}
			</uui-input>
		`;
	}

	#renderLinkUrlInputReadOnly() {
		if (!this.value.link.unique || !this.value.link.url) return nothing;
		return html`<uui-input readonly value=${this.value.link.url}></uui-input>`;
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
					.value=${this.value.link.queryString ?? ''}
					@change=${this.#onLinkAnchorInput}></uui-input>
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
