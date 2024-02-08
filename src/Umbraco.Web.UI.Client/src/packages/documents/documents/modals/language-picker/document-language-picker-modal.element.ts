import type {
	UmbDocumentLanguagePickerModalValue,
	UmbDocumentLanguagePickerModalData,
} from './document-language-picker-modal.token.js';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageItemModel } from '@umbraco-cms/backoffice/language';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-language-picker-modal')
export class UmbDocumentLanguagePickerModalElement extends UmbModalBaseElement<
	UmbDocumentLanguagePickerModalData,
	UmbDocumentLanguagePickerModalValue
> {
	@state()
	private _languages: Array<UmbLanguageItemModel> = [];

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({ skip: 0, take: 1000 });
		this._languages = data?.items ?? [];
	}

	get #filteredLanguages() {
		if (this.data?.filter) {
			return this._languages.filter(this.data.filter);
		} else {
			return this._languages;
		}
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
				this.#filteredLanguages,
				(item) => item.unique,
				(item) => html`
					<uui-menu-item
						label=${item.name ?? ''}
						selectable
						@selected=${() => this.#selectionManager.select(item.unique)}
						@deselected=${() => this.#selectionManager.deselect(item.unique)}
						?selected=${this.#selectionManager.isSelected(item.unique)}>
						<uui-icon slot="icon" name="icon-globe"></uui-icon>
						<div class="label" slot="label">
							<strong>${item.name}</strong>
							<div class="label-status">
								${this.localize.term(
									this.data?.publishedVariants.includes(item.unique) ? 'content_published' : 'content_unpublished',
								)}
							</div>
						</div>
					</uui-menu-item>
				`,
			)}
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
				color: rgba(var(--uui-color-text), 0.6);
			}
		`,
	];
}

export default UmbDocumentLanguagePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-language-picker-modal': UmbDocumentLanguagePickerModalElement;
	}
}
