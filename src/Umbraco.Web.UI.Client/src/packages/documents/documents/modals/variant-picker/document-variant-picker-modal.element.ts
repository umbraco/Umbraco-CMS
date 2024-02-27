import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import type {
	UmbDocumentVariantPickerModalValue,
	UmbDocumentVariantPickerModalData,
} from './document-variant-picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-variant-picker-modal')
export class UmbDocumentVariantPickerModalElement extends UmbModalBaseElement<
	UmbDocumentVariantPickerModalData,
	UmbDocumentVariantPickerModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_selection: Array<string> = [];

	constructor() {
		super();
		this.observe(this.#selectionManager.selection, (selection) => {
			this._selection = selection;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#setInitialSelection();
	}

	async #setInitialSelection() {
		const selected = this.value?.selection ?? [];

		if (selected.length === 0) {
			// TODO: Make it possible to use consume context without callback. [NL]
			const ctrl = this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (appLanguageContext) => {});
			const context = await ctrl.asPromise();
			const appCulture = context.getAppCulture();
			// If the app language is one of the options, select it by default:
			if (appCulture && modalData.options.some((o) => o.language.unique === appCulture)) {
				selected.push(new UmbVariantId(appCulture, null));
			}
			ctrl.destroy();
		}

		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		if (this.data?.type !== 'unpublish') {
			this.#selectMandatoryVariants();
		}
	}

	#selectMandatoryVariants() {
		this.data?.options.forEach((variant) => {
			if (variant.language?.isMandatory) {
				this.#selectionManager.select(variant.unique);
			}
		});
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
				this.data?.options ?? [],
				(option) => option.unique,
				(option) => html`
					<uui-menu-item
						selectable
						label=${option.variant?.name ?? option.language.name}
						@selected=${() => this.#selectionManager.select(option.unique)}
						@deselected=${() => this.#selectionManager.deselect(option.unique)}
						?selected=${this._selection.includes(option.language.unique)}>
						<uui-icon slot="icon" name="icon-globe"></uui-icon>
						${this.#renderLabel(option)}
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

	#renderLabel(option: UmbDocumentVariantOptionModel) {
		return html`<div class="label" slot="label">
			<strong
				>${option.variant?.segment ? option.variant.segment + ' - ' : ''}${option.variant?.name ??
				option.language.name}</strong
			>
			<div class="label-status">${this.#renderVariantStatus(option)}</div>
			${option.language.isMandatory && option.variant?.state !== UmbDocumentVariantState.PUBLISHED
				? html`<div class="label-status">
						<umb-localize key="languages_mandatoryLanguage">Mandatory language</umb-localize>
				  </div>`
				: ''}
		</div>`;
	}

	#renderVariantStatus(option: UmbDocumentVariantOptionModel) {
		switch (option.variant?.state) {
			case UmbDocumentVariantState.PUBLISHED:
				return this.localize.term('content_published');
			case UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES:
				return this.localize.term('content_publishedPendingChanges');
			case UmbDocumentVariantState.DRAFT:
				return this.localize.term('content_unpublished');
			case UmbDocumentVariantState.NOT_CREATED:
			default:
				return this.localize.term('content_notCreated');
		}
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 400px;
				max-width: 90vw;
			}
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
