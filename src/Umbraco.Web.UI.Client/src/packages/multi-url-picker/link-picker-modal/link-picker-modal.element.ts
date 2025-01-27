import type { UmbLinkPickerLink } from './types.js';
import type {
	UmbLinkPickerConfig,
	UmbLinkPickerModalData,
	UmbLinkPickerModalValue,
} from './link-picker-modal.token.js';
import { css, customElement, html, nothing, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { isUmbracoFolder, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { umbBindToValidation, UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import { umbConfirmModal, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';
import { UmbMediaDetailRepository } from '@umbraco-cms/backoffice/media';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UUIBooleanInputEvent, UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

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

	@query('umb-input-document')
	private _documentPickerElement?: UmbInputDocumentElement;

	@query('umb-input-media')
	private _mediaPickerElement?: UmbInputMediaElement;

	@query('#link-anchor', true)
	private _linkAnchorInput?: UUIInputElement;

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
	}

	protected override firstUpdated() {
		this._linkAnchorInput?.addValidator(
			'valueMissing',
			() => this.localize.term('linkPicker_modalAnchorValidationMessage'),
			() => !this.value.link.url && !this.value.link.queryString,
		);
	}

	async #getMediaTypes() {
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
					const documentRepository = new UmbDocumentDetailRepository(this);
					const { data: documentData } = await documentRepository.requestByUnique(unique);
					if (documentData) {
						icon = documentData.documentType.icon;
						name = documentData.variants[0].name;
						url = documentData.urls[0]?.url ?? '';
					}
					break;
				}
				case 'media': {
					const mediaRepository = new UmbMediaDetailRepository(this);
					const { data: mediaData } = await mediaRepository.requestByUnique(unique);
					if (mediaData) {
						icon = mediaData.mediaType.icon;
						name = mediaData.variants[0].name;
						url = mediaData.urls[0].url;
					}
					break;
				}
				default:
					break;
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

		await this.#validationContext.validate();
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
					this.modalContext?.data.isNew ? 'defaultdialogs_addLink' : 'defaultdialogs_updateLink',
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
						label=${this.localize.term(this.modalContext?.data.isNew ? 'general_add' : 'general_update')}
						?disabled=${!this.value.link.type}
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
				mandatory>
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
					label=${this.localize.term('general_document')}
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
		if (this.value.link.type !== 'external') return nothing;
		return html`
			<uui-input
				data-mark="input:url"
				label=${this.localize.term('placeholders_enterUrl')}
				placeholder=${this.localize.term('placeholders_enterUrl')}
				.value=${this.value.link.url ?? ''}
				?disabled=${!!this.value.link.unique}
				?required=${this._config.hideAnchor}
				@change=${this.#onLinkUrlInput}
				${umbBindToValidation(this)}>
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
					id="link-anchor"
					label=${this.localize.term('placeholders_anchor')}
					placeholder=${this.localize.term('placeholders_anchor')}
					.value=${this.value.link.queryString ?? ''}
					@change=${this.#onLinkAnchorInput}
					${umbBindToValidation(this)}></uui-input>
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
