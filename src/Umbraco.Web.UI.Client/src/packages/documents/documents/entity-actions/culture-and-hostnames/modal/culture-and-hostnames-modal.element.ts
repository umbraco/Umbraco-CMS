import { UmbDocumentCultureAndHostnamesRepository } from '../repository/index.js';
import type {
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue,
} from './culture-and-hostnames-modal.token.js';
import { css, customElement, html, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { DomainPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UUIInputEvent, UUIPopoverContainerElement, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-culture-and-hostnames-modal')
export class UmbCultureAndHostnamesModalElement extends UmbModalBaseElement<
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue
> {
	#documentRepository = new UmbDocumentCultureAndHostnamesRepository(this);
	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	#unique?: string | null;

	@state()
	private _languageModel: Array<UmbLanguageDetailModel> = [];

	@state()
	private _defaultIsoCode?: string | null;

	@state()
	private _domains: Array<DomainPresentationModel> = [];

	@query('#more-options')
	popoverContainerElement?: UUIPopoverContainerElement;

	// Init

	override firstUpdated() {
		this.#unique = this.data?.unique;
		this.#requestLanguages();
		this.#readDomains();
	}

	async #readDomains() {
		if (!this.#unique) return;
		const { data } = await this.#documentRepository.readCultureAndHostnames(this.#unique);

		if (!data) return;
		this._defaultIsoCode = data.defaultIsoCode;
		this._domains = data.domains;
	}

	async #requestLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({});
		if (!data) return;
		this._languageModel = data.items;
	}

	async #handleSave() {
		this.value = { defaultIsoCode: this._defaultIsoCode, domains: this._domains };
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
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, isoCode } : domain));
	}

	#onChangeDomainHostname(e: UUIInputEvent, index: number) {
		const domainName = e.target.value as string;
		this._domains = this._domains.map((domain, i) => (index === i ? { ...domain, domainName } : domain));
	}

	async #onRemoveDomain(index: number) {
		this._domains = this._domains.filter((d, i) => index !== i);
	}

	#onAddDomain(useCurrentDomain?: boolean) {
		const defaultModel = this._languageModel.find((model) => model.isDefault);
		if (useCurrentDomain) {
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.popoverContainerElement?.hidePopover();
			this._domains = [...this._domains, { isoCode: defaultModel?.unique ?? '', domainName: window.location.host }];
		} else {
			this._domains = [...this._domains, { isoCode: defaultModel?.unique ?? '', domainName: '' }];
		}
	}

	// Renders

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_assigndomain')}>
				${this.#renderCultureSection()} ${this.#renderDomainSection()}
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

	#renderCultureSection() {
		return html`
			<uui-box headline=${this.localize.term('assignDomain_setLanguage')}>
				<uui-label for="select">${this.localize.term('assignDomain_language')}</uui-label>
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
			</uui-box>
		`;
	}

	#renderDomainSection() {
		return html`
			<uui-box headline=${this.localize.term('assignDomain_setDomains')}>
				<umb-localize key="assignDomain_domainHelpWithVariants">
					Valid domain names are: "example.com", "www.example.com", "example.com:8080", or
					"https://www.example.com/".<br />Furthermore also one-level paths in domains are supported, eg.
					"example.com/en" or "/en".
				</umb-localize>
				${this.#renderDomains()} ${this.#renderAddNewDomainButton()}
			</uui-box>
		`;
	}

	#renderDomains() {
		if (!this._domains?.length) return;
		return html`
			<div id="domains">
				${repeat(
					this._domains,
					(domain) => domain.isoCode,
					(domain, index) => html`
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
					`,
				)}
			</div>
		`;
	}

	#renderLanguageModelOptions() {
		return html`${repeat(
			this._languageModel,
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
			uui-button-group {
				width: 100%;
			}

			uui-box:first-child {
				margin-bottom: var(--uui-size-layout-1);
			}

			#dropdown {
				flex-grow: 0;
			}

			#domains {
				margin-top: var(--uui-size-layout-1);
				margin-bottom: var(--uui-size-2);
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				grid-gap: var(--uui-size-1);
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
