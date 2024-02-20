import { type UmbDocumentVariantModel, UmbDocumentVariantState } from '../../types.js';
import type {
	UmbDocumentVariantPickerModalValue,
	UmbDocumentVariantPickerModalData,
} from './document-variant-picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-variant-picker-modal')
export class UmbDocumentVariantPickerModalElement extends UmbModalBaseElement<
	UmbDocumentVariantPickerModalData,
	UmbDocumentVariantPickerModalValue
> {
	#selectionManager = new UmbSelectionManager(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	get #headline(): string {
		switch (this.data?.type) {
			case 'publish':
				return 'content_readyToPublish';
			case 'unpublish':
				return 'content_unpublish';
			case 'schedule':
				return 'content_readyToPublish';
			default:
				return 'content_readyToSave';
		}
	}

	get #subtitle(): string {
		switch (this.data?.type) {
			case 'publish':
				return 'content_variantsToPublish';
			case 'unpublish':
				return 'content_languagesToUnpublish';
			case 'schedule':
				return 'content_languagesToSchedule';
			default:
				return 'content_variantsToSave';
		}
	}

	get #confirmLabel(): string {
		switch (this.data?.type) {
			case 'publish':
				return 'buttons_saveAndPublish';
			case 'unpublish':
				return 'actions_unpublish';
			case 'schedule':
				return 'buttons_schedulePublish';
			default:
				return 'buttons_saveAndClose';
		}
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline=${this.localize.term(this.#headline)}>
			<p id="subtitle">${this.localize.term(this.#subtitle)}</p>
			${repeat(
				this.data?.variants ?? [],
				(item) => item.culture,
				(item) => html`
					<uui-menu-item
						label=${item.name}
						selectable
						@selected=${() => this.#selectionManager.select(item.culture)}
						@deselected=${() => this.#selectionManager.deselect(item.culture)}
						?selected=${this.#selectionManager.isSelected(item.culture)}>
						<uui-icon slot="icon" name="icon-globe"></uui-icon>
						<div class="label" slot="label">
							<strong>${item.name}</strong>
							<div class="label-status">${this.#renderVariantStatus(item)}</div>
						</div>
					</uui-menu-item>
				`,
			)}
			${this.data?.type === 'publish' ? html`<p>${this.localize.term('content_variantsWillBeSaved')}</p>` : ''}

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term(this.#confirmLabel)}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	#renderVariantStatus(variant: UmbDocumentVariantModel) {
		switch (variant.state) {
			case UmbDocumentVariantState.PUBLISHED:
				return this.localize.term('content_published');
			case UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES:
				return this.localize.term('content_publishedPendingChanges');
			case UmbDocumentVariantState.NOT_CREATED:
			case UmbDocumentVariantState.DRAFT:
			default:
				return this.localize.term('content_unpublished');
		}
	}

	static styles = [
		UmbTextStyles,
		css`
			#subtitle {
				margin-top: 0;
			}
			.label {
				padding: 0.5rem 0;
			}
			.label-status {
				font-size: 0.8rem;
			}
		`,
	];
}

export default UmbDocumentVariantPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-variant-picker-modal': UmbDocumentVariantPickerModalElement;
	}
}
