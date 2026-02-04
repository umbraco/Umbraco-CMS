import { UmbDocumentCultureAndHostnamesRepository } from '../repository/index.js';
import type {
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue,
} from './culture-and-hostnames-modal.token.js';
import {
	css,
	customElement,
	html,
	query,
	repeat,
	state,
	when,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UUIInputEvent, UUIPopoverContainerElement, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbId } from '@umbraco-cms/backoffice/id';

interface UmbDomainPresentationModel {
	unique: string;
	domainName: string;
	isoCode: string;
}
@customElement('umb-culture-and-hostnames-modal')
export class UmbCultureAndHostnamesModalElement extends UmbModalBaseElement<
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue
> {
	#sorter = new UmbSorterController(this, {
		getUniqueOfElement: (element) => {
			return element.getAttribute('data-sort-entry-id');
		},
		getUniqueOfModel: (modelEntry: UmbDomainPresentationModel) => {
			return modelEntry.unique;
		},
		itemSelector: '.hostname-item',
		containerSelector: '#sorter-wrapper',
		onChange: ({ model }) => {
			const oldValue = this._domains;
			this._domains = model;
			this.requestUpdate('_domains', oldValue);
		},
	});

	#documentRepository = new UmbDocumentCultureAndHostnamesRepository(this);
	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	#unique?: string | null;

	// Initialize as undefined to track loading state
	// When both _languageModel and _domains are set (non-undefined), loading is complete
	@state()
	private _languageModel: Array<UmbLanguageDetailModel> | undefined = undefined;

	@state()
	private _defaultIsoCode?: string | null;

	// Initialize as undefined to track loading state
	// When both _languageModel and _domains are set (non-undefined), loading is complete
	@state()
	private _domains: Array<UmbDomainPresentationModel> | undefined = undefined;

	@query('#more-options')
	popoverContainerElement?: UUIPopoverContainerElement;

	// Init

	override willUpdate(changedProperties: PropertyValues) {
		if (changedProperties.has('_domains') && this._domains) {
			// Update sorter whenever _domains changes and is defined
			this.#sorter.setModel(this._domains);
		}
	}

	constructor() {
		super();
		this.#sorter.disable();
	}

	override firstUpdated() {
		this.#unique = this.data?.unique;
		this.#requestLanguages();
		this.#readDomains();
	}

	async #readDomains() {
		if (!this.#unique) return;
		const { data } = await this.#documentRepository.readCultureAndHostnames(this.#unique);

		if (!data) {
			// Set to empty array to indicate loading is complete
			this._domains = [];
			return;
		}
		this._defaultIsoCode = data.defaultIsoCode;
		this._domains = data.domains.map((domain) => ({ ...domain, unique: UmbId.new() }));
		this.#sorter.enable();
	}

	async #requestLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({ take: 999 });
		// Set to empty array if no data, to indicate loading is complete
		this._languageModel = data?.items ?? [];
	}

	async #handleSave() {
		// Ensure data is loaded before saving
		if (!this._domains) return;
		const cleanDomains = this._domains.map((domain) => ({ domainName: domain.domainName, isoCode: domain.isoCode }));
		this.value = { defaultIsoCode: this._defaultIsoCode, domains: cleanDomains };
		const { error } = await this.#documentRepository.updateCultureAndHostnames(this.#unique!, this.value);

		if (!error) {
			this._submitModal();
		}
	}

	#handleCancel() {
		this._rejectModal();
	}

	// Events

	#onChangeLanguage(e: UUISelectEvent) {
		const defaultIsoCode = e.target.value as string;
		if (defaultIsoCode === 'inherit') {
			this._defaultIsoCode = null;
		} else {
			this._defaultIsoCode = defaultIsoCode;
		}
	}

	#onChangeDomainLanguage(e: UUISelectEvent, index: number) {
		const isoCode = e.target.value as string;
		// Only update if _domains is defined
		if (!this._domains) return;
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, isoCode } : domain));
	}

	#onChangeDomainHostname(e: UUIInputEvent, index: number) {
		const domainName = e.target.value as string;
		// Only update if _domains is defined
		if (!this._domains) return;
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, domainName } : domain));
	}

	async #onRemoveDomain(index: number) {
		// Only update if _domains is defined
		if (!this._domains) return;
		this._domains = this._domains.filter((d, i) => index !== i);
	}

	#onAddDomain(useCurrentDomain?: boolean) {
		// Only add domain if both _domains and _languageModel are defined
		if (!this._domains || !this._languageModel) return;
		const defaultModel = this._languageModel.find((model) => model.isDefault);
		if (useCurrentDomain) {
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.popoverContainerElement?.hidePopover();
			this._domains = [
				...this._domains,
				{ isoCode: defaultModel?.unique ?? '', domainName: window.location.host, unique: UmbId.new() },
			];

			this.#focusNewItem();
		} else {
			this._domains = [...this._domains, { isoCode: defaultModel?.unique ?? '', domainName: '', unique: UmbId.new() }];

			this.#focusNewItem();
		}
	}

	async #focusNewItem() {
		await this.updateComplete;
		const items = this.shadowRoot?.querySelectorAll('div.hostname-item') as NodeListOf<HTMLElement>;
		const newItem = items[items.length - 1];
		const firstInput = newItem?.querySelector('uui-input') as HTMLElement;
		firstInput?.focus();
	}

	// Renders

	// Check if data is still loading
	// Returns true when both _domains and _languageModel have been set (non-undefined)
	#isLoading(): boolean {
		return this._domains === undefined || this._languageModel === undefined;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_assigndomain')}>
				<!-- Display loader while _domains and _languageModel are being loaded -->
				${when(
					this.#isLoading(),
					() => html`
						<div id="loader-container">
							<uui-loader></uui-loader>
						</div>
					`,
					() => html`
						<uui-box>
							<umb-property-layout
								label=${this.localize.term('assignDomain_language')}
								description=${this.localize.term('assignDomain_setLanguageHelp')}
								orientation="vertical"
								><div slot="editor">
									<uui-combobox
										id="select"
										label=${this.localize.term('assignDomain_language')}
										.value=${(this._defaultIsoCode as string) ?? 'inherit'}
										@change=${this.#onChangeLanguage}>
										<uui-combobox-list>
											<uui-combobox-list-option .value=${'inherit'}>
												${this.localize.term('assignDomain_inherit')}
											</uui-combobox-list-option>
											${this.#renderLanguageModelOptions()}
										</uui-combobox-list>
									</uui-combobox>
								</div>
							</umb-property-layout>
						</uui-box>
						<uui-box>
							<umb-property-layout
								label=${this.localize.term('assignDomain_setDomains')}
								description=${this.localize.term('assignDomain_domainHelpWithVariants')}
								orientation="vertical"
								><div slot="editor">
									${this.#renderDomains()} ${this.#renderAddNewDomainButton()}
								</div></umb-property-layout
							>
						</uui-box>
					`,
				)}
				<uui-button
					slot="actions"
					id="close"
					label=${this.localize.term('general_cancel')}
					@click=${this.#handleCancel}></uui-button>
				<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="positive"
					label=${this.localize.term('buttons_save')}
					@click=${this.#handleSave}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderDomains() {
		return html`
			<div id="sorter-wrapper">
				${repeat(
					this._domains ?? [],
					(domain) => domain.unique,
					(domain, index) => html`
						<div class="hostname-item" data-sort-entry-id=${domain.unique}>
							<uui-icon name="icon-grip" class="handle"></uui-icon>
							<div class="hostname-wrapper">
								<uui-input
									label=${this.localize.term('assignDomain_domain')}
									.value=${domain.domainName}
									@change=${(e: UUIInputEvent) => this.#onChangeDomainHostname(e, index)}></uui-input>
								<uui-combobox
									.value=${domain.isoCode as string}
									label=${this.localize.term('assignDomain_language')}
									@change=${(e: UUISelectEvent) => this.#onChangeDomainLanguage(e, index)}>
									<uui-combobox-list> ${this.#renderLanguageModelOptions()} </uui-combobox-list>
								</uui-combobox>
								<uui-button
									look="outline"
									color="danger"
									label=${this.localize.term('assignDomain_remove')}
									@click=${() => this.#onRemoveDomain(index)}>
									<uui-icon name="icon-trash"></uui-icon>
								</uui-button>
							</div>
						</div>
					`,
				)}
			</div>
		`;
	}

	#renderLanguageModelOptions() {
		return html`${repeat(
			this._languageModel ?? [],
			(model) => model.unique,
			(model) => html`<uui-combobox-list-option .value=${model.unique}>${model.name}</uui-combobox-list-option>`,
		)}`;
	}

	#renderAddNewDomainButton() {
		return html`
			<uui-button-group>
				<uui-button
					label=${this.localize.term('assignDomain_addNew')}
					look="placeholder"
					@click=${() => this.#onAddDomain()}></uui-button>
				<uui-button
					id="dropdown"
					label=${this.localize.term('buttons_select')}
					look="placeholder"
					popovertarget="more-options">
					<uui-icon name="icon-navigation-down"></uui-icon>
				</uui-button>
				<uui-popover-container id="more-options" placement="bottom-end">
					<umb-popover-layout>
						<uui-button
							label=${this.localize.term('assignDomain_addCurrent')}
							@click=${() => this.#onAddDomain(true)}></uui-button>
					</umb-popover-layout>
				</uui-popover-container>
			</uui-button-group>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#loader-container {
				display: flex;
				justify-content: center;
				align-items: center;
				min-height: 200px;
			}

			umb-property-layout[orientation='vertical'] {
				padding: 0;
			}
			uui-button-group {
				width: 100%;
			}

			uui-box:first-child {
				margin-bottom: var(--uui-size-layout-1);
			}

			#dropdown {
				flex-grow: 0;
			}

			.hostname-item {
				position: relative;
				display: flex;
				gap: var(--uui-size-1);
				align-items: center;
			}

			.hostname-wrapper {
				position: relative;
				flex: 1;
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				gap: var(--uui-size-1);
			}

			#sorter-wrapper {
				margin-bottom: var(--uui-size-2);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-1);
			}

			.handle {
				cursor: grab;
			}

			.handle:active {
				cursor: grabbing;
			}
			#action {
				display: block;
			}

			.--umb-sorter-placeholder {
				position: relative;
				visibility: hidden;
			}
			.--umb-sorter-placeholder::after {
				content: '';
				position: absolute;
				inset: 0px;
				border-radius: var(--uui-border-radius);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbCultureAndHostnamesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-culture-and-hostnames-modal': UmbCultureAndHostnamesModalElement;
	}
}
