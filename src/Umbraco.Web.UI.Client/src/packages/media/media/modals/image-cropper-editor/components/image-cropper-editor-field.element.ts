import { UmbInputImageCropperFieldElement } from '../../../components/input-image-cropper/image-cropper-field.element.js';
import type { UmbImageCropChangeEvent } from '../../../components/input-image-cropper/crop-change.event.js';
import type { UmbImageCropperElement } from '../../../components/input-image-cropper/image-cropper.element.js';
import type { UmbImageCropperCrop } from '../../../components/index.js';
import type { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-image-cropper-editor-field')
export class UmbImageCropperEditorFieldElement extends UmbInputImageCropperFieldElement {
	@property({ type: Boolean, attribute: 'enable-alt-text-per-crop', reflect: true })
	enableAltTextPerCrop = false;

	/** When true, image selection and crop/focal-point editing are disabled (shared property on a non-default culture tab). */
	@property({ type: Boolean, attribute: 'readonly-media' })
	readonlyMedia = false;

	@property({ type: String, attribute: 'alt-text-mode' })
	altTextMode: 'off' | 'altText' | 'decorative' = 'off';

	@property({ type: String, attribute: 'alt-text' })
	altText: string = '';

	/** ISO culture code being edited (e.g. 'en', 'da'). When set, shows a culture label on the alt text section. */
	@property({ type: String })
	culture?: string;

	@state()
	private _announcement = '';

	@state()
	private _altTextHelpVisible = false;

	/**
	 * Alias of the crop currently selected for alt-text editing. Separate from `currentCrop` so that
	 * in readonlyMedia mode we can highlight a crop and show its alt text without entering crop-editing mode.
	 */
	@state()
	private _selectedCropAlias?: string;

	override render() {
		const hasCropAltText = this.enableAltTextPerCrop && !!(this.currentCrop || this._selectedCropAlias);
		const showSkipLink = this.altTextMode === 'altText' && (!this.currentCrop || hasCropAltText);
		return html`
			<div class="sr-only" role="status" aria-live="polite" aria-atomic="true">${this._announcement}</div>
			${showSkipLink
				? html`
						<uui-visually-hidden>
							<button class="skip-link" @click=${this.#skipToAltText}>
								${this.localize.term('mediaPicker_skipToAltText')}
							</button>
						</uui-visually-hidden>
					`
				: nothing}
			${super.render()}
		`;
	}

	override focus(options?: FocusOptions) {
		const skipLink = this.shadowRoot?.querySelector<HTMLElement>('.skip-link');
		if (skipLink) {
			skipLink.focus(options);
			return;
		}
		const cropper = this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper');
		if (cropper) {
			cropper.focus(options);
			return;
		}
		const focusSetter = this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-focus-setter');
		if (focusSetter && !this.hideFocalPoint) {
			focusSetter.focus(options);
			return;
		}
		this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-preview')?.focus(options);
	}

	/**
	 * In normal mode: enter crop editing (sets currentCrop via super) and track _selectedCropAlias.
	 * In readonlyMedia mode: only set _selectedCropAlias so the crop's alt text is shown in the
	 * side panel without activating the crop editor.
	 */
	protected override onCropClick(crop: UmbImageCropperCrop) {
		this._selectedCropAlias = crop.alias;
		if (!this.readonlyMedia) {
			super.onCropClick(crop);
			const cropLabel = crop.label ?? crop.alias;
			this._announcement = this.localize.term('mediaPicker_cropSelected', [cropLabel]);
			this.updateComplete.then(async () => {
				const skipLink = this.shadowRoot?.querySelector<HTMLElement>('.skip-link');
				if (skipLink) {
					skipLink.focus();
					return;
				}
				const cropper = this.shadowRoot?.querySelector<UmbImageCropperElement>('umb-image-cropper');
				if (cropper) {
					await cropper.updateComplete;
					cropper.focus();
				}
			});
		} else if (this.enableAltTextPerCrop) {
			// Readonly media + per-crop alt text: focus the alt text field directly
			this.updateComplete.then(() => this.#skipToAltText());
		}
	}

	protected override renderMain() {
		if (this.currentCrop) {
			// currentCrop is only set when !readonlyMedia (see onCropClick override above)
			return html`
				<umb-image-cropper
					.focalPoint=${this.focalPoint}
					.src=${this.source}
					.value=${this.currentCrop}
					?hideFocalPoint=${this.hideFocalPoint}
					?hideZoom=${this.hideZoomCrop}
					@imagecrop-change=${this.#onCropSave}>
					${this.enableAltTextPerCrop
						? html`<div slot="actions-prefix">${this.#renderCropAltText(this.currentCrop)}</div>`
						: nothing}
				</umb-image-cropper>
			`;
		}
		const selectedCrop = this._selectedCropAlias
			? this.crops.find((c) => c.alias === this._selectedCropAlias)
			: undefined;

		// In readonlyMedia mode with a crop selected, show a display-only crop preview alongside the alt text.
		if (this.readonlyMedia && selectedCrop) {
			return html`
				<umb-image-cropper
					.focalPoint=${this.focalPoint}
					.src=${this.source}
					.value=${selectedCrop}
					display-only
					hideZoom
					hideFocalPoint>
				</umb-image-cropper>
				${this.enableAltTextPerCrop ? this.#renderCropAltText(selectedCrop) : nothing}
			`;
		}

		return html`
			${super.renderMain()}
			${this.enableAltTextPerCrop && selectedCrop ? this.#renderCropAltText(selectedCrop) : nothing}
			${this.altTextMode === 'altText' ? this.#renderSingleAltTextField() : nothing}
		`;
	}

	#onCropSave = (event: UmbImageCropChangeEvent) => {
		this._onCropChange(event);
		this._selectedCropAlias = undefined;
		this._announcement = this.localize.term('mediaPicker_focalPointViewSelected');
		this.updateComplete.then(() => {
			const focusSetter = this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-focus-setter');
			if (focusSetter && !this.hideFocalPoint) {
				focusSetter.focus();
			} else {
				this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-preview')?.focus();
			}
		});
	};

	#skipToAltText() {
		const hasCropAltText = this.enableAltTextPerCrop && !!(this.currentCrop || this._selectedCropAlias);
		const altFieldId = hasCropAltText ? '#crop-alt-text' : '#single-alt-text';
		const altField = this.shadowRoot?.querySelector<HTMLElement>(altFieldId);
		const textarea = altField?.querySelector<HTMLElement>('uui-textarea');
		(textarea ?? altField)?.focus();
	}

	#resetCurrentCrop() {
		this.currentCrop = undefined;
		this._selectedCropAlias = undefined;
		this._announcement = this.localize.term('mediaPicker_focalPointViewSelected');
		this.updateComplete.then(() => {
			const focusSetter = this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-focus-setter');
			if (focusSetter && !this.hideFocalPoint) {
				focusSetter.focus();
			}
		});
	}

	override renderSide() {
		if (this.readonlyMedia && !this.enableAltTextPerCrop) return nothing;
		if (!this.value || !this.crops?.length) return nothing;

		return html`
			<umb-image-cropper-preview
				.label=${this.localize.term('general_media')}
				.actionLabel=${this.readonlyMedia
					? this.localize.term('mediaPicker_altTextDefaultLabel')
					: this.localize.term('mediaPicker_editFocalPointLabel')}
				?active=${!this.currentCrop && !this._selectedCropAlias}
				@click=${this.#resetCurrentCrop}>
			</umb-image-cropper-preview>
			${repeat(
				this.crops,
				(crop) => crop.alias + JSON.stringify(crop.coordinates),
				(crop) => this.#renderCropSideItem(crop),
			)}
		`;
	}

	#renderCropSideItem(crop: UmbImageCropperCrop) {
		return html`
			<umb-image-cropper-preview
				.crop=${crop}
				.focalPoint=${this.focalPoint}
				.src=${this.source}
				.actionLabel=${this.localize.term('mediaPicker_editCropLabel', [crop.label ?? crop.alias])}
				?active=${this._selectedCropAlias === crop.alias}
				@click=${() => this.onCropClick(crop)}>
			</umb-image-cropper-preview>
		`;
	}

	#renderSingleAltTextField() {
		const showDefaultLabel = this.enableAltTextPerCrop && !!this._selectedCropAlias;
		const labelContent = this.culture
			? html`<umb-localize key="mediaPicker_altTextCultureLabel" .args=${[this.culture]}
					>Alternative text (${this.culture})</umb-localize
				>`
			: showDefaultLabel
				? html`<umb-localize key="mediaPicker_altTextDefaultLabel">Default alternative text</umb-localize>`
				: html`<umb-localize key="mediaPicker_altTextLabel">Alternative text</umb-localize>`;
		return html`
			<div id="single-alt-text" class="alt-text-section" tabindex="-1">
				<div class="alt-text-field">
					<label for="main-alt-text" class="alt-text-label">${labelContent}</label>
					<button
						type="button"
						class="alt-text-help-toggle"
						aria-expanded=${this._altTextHelpVisible}
						@click=${() => {
							this._altTextHelpVisible = !this._altTextHelpVisible;
						}}>
						<umb-localize key="mediaPicker_altTextHelpLabel">How to write alt text</umb-localize>
					</button>
					<p class="alt-text-help-body" ?hidden=${!this._altTextHelpVisible}>
						<umb-localize key="mediaPicker_altTextHelpContextual"
							>Describe what the image conveys in context.</umb-localize
						>
					</p>
					<uui-textarea
						id="main-alt-text"
						label=${this.localize.term('mediaPicker_altTextLabel')}
						.value=${this.altText}
						placeholder=${this.localize.term('mediaPicker_altTextPlaceholder')}
						@change=${this.#onSingleAltTextChange}>
					</uui-textarea>
				</div>
			</div>
		`;
	}

	#onSingleAltTextChange(event: Event) {
		const textarea = event.target as UUITextareaElement;
		this.altText = textarea.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderCropAltText(crop: UmbImageCropperCrop) {
		const cropLabel = crop.label ?? crop.alias;
		const inputId = `crop-alt-input-${crop.alias}`;
		const textareaLabel = this.culture
			? `${this.localize.term('mediaPicker_altTextCultureLabel', [this.culture])} — ${cropLabel}`
			: `${this.localize.term('mediaPicker_altTextLabel')} — ${cropLabel}`;
		const labelContent = this.culture
			? html`<umb-localize key="mediaPicker_altTextCultureLabel" .args=${[this.culture]}
						>Alternative text (${this.culture})</umb-localize
					>
					&mdash; <em>${cropLabel}</em>`
			: html`<umb-localize key="mediaPicker_altTextLabel">Alternative text</umb-localize> &mdash;
					<em>${cropLabel}</em>`;
		return html`
			<div id="crop-alt-text" class="alt-text-section" tabindex="-1">
				<label for=${inputId} class="alt-text-label">${labelContent}</label>
				<uui-textarea
					id=${inputId}
					label=${textareaLabel}
					.value=${crop.altText ?? ''}
					placeholder=${this.localize.term('mediaPicker_altTextPlaceholder')}
					@change=${(e: Event) => this.#onCropAltTextChange(crop.alias, e)}>
				</uui-textarea>
			</div>
		`;
	}

	#onCropAltTextChange(alias: string, event: Event) {
		if (!this.value) return;
		const textarea = event.target as UUITextareaElement;
		const updatedCrops = this.crops.map((c) => (c.alias === alias ? { ...c, altText: textarea.value as string } : c));
		this.value = { ...this.value, crops: updatedCrops };
		// Keep currentCrop in sync so umb-image-cropper emits the updated altText when the user clicks Save
		if (this.currentCrop?.alias === alias) {
			this.currentCrop = { ...this.currentCrop, altText: textarea.value as string };
		}
		this.dispatchEvent(new UmbChangeEvent());
	}

	static override styles = [
		...super.styles,
		UmbTextStyles,
		css`
			:host {
				position: relative;
			}

			uui-visually-hidden {
				position: absolute;
				top: 0;
				left: 0;
				z-index: 10;
			}

			.skip-link {
				display: block;
				padding: var(--uui-size-space-2) var(--uui-size-space-3);
				background: var(--uui-color-surface);
				border: 2px solid var(--uui-color-focus);
				border-radius: var(--uui-border-radius);
				cursor: pointer;
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-interactive);
				white-space: nowrap;
				font-family: inherit;
			}

			#main {
				max-width: unset;
				min-width: unset;
				overflow-y: auto;
			}

			umb-image-cropper {
				flex: 1;
				min-height: 250px;
			}

			#side {
				flex: none;
			}

			.alt-text-section {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
				flex-shrink: 0;
				margin-top: var(--uui-size-space-2);
			}

			.alt-text-label {
				font-size: var(--uui-type-small-size);
				font-weight: bold;
			}

			.alt-text-field {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			.alt-text-help-toggle {
				display: inline;
				background: none;
				border: none;
				padding: 0;
				margin: 0;
				cursor: pointer;
				font-size: var(--uui-type-small-size);
				font-family: inherit;
				color: var(--uui-color-text-alt);
				text-decoration: underline;
				text-decoration-style: dotted;
				text-underline-offset: 2px;
				text-align: left;
			}

			.alt-text-help-toggle:hover {
				color: var(--uui-color-text);
			}

			.alt-text-help-toggle:focus-visible {
				color: var(--uui-color-text);
				outline: 2px solid var(--uui-color-focus);
				outline-offset: 2px;
				border-radius: 2px;
			}

			.alt-text-help-body {
				margin: 0;
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-editor-field': UmbImageCropperEditorFieldElement;
	}
}
